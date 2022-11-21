namespace Pidp.Features.AccessRequests;

using DomainResults.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

using Pidp.Data;
using Pidp.Infrastructure.HttpClients.Keycloak;
using Pidp.Kafka.Interfaces;
using Pidp.Models;
using Pidp.Models.Lookups;

public class DigitalEvidence
{
    public class Command : ICommand<IDomainResult>
    {
        public int PartyId { get; set; }
        public string OrganizationType { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string ParticipantId { get; set; } = string.Empty;
        public List<AssignedRegion> AssignedRegions { get; set; } = new List<AssignedRegion>();
    }
    public enum UserType
    {
        BCPS = 1,
        OutOfCustody,
        Police,
        Lawyer,
        None
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            this.RuleFor(x => x.OrganizationName).NotEmpty();
            this.RuleFor(x => x.OrganizationType).NotEmpty();
            this.RuleFor(x => x.ParticipantId).NotEmpty();
            this.RuleFor(x => x.AssignedRegions).ForEach(x => x.NotEmpty());
            this.RuleFor(x => x.PartyId).GreaterThan(0);
        }
    }

    public class CommandHandler : ICommandHandler<Command, IDomainResult>
    {
        private readonly IClock clock;
        private readonly IKeycloakAdministrationClient keycloakClient;
        private readonly ILogger logger;
        private readonly PidpConfiguration config;
        private readonly PidpDbContext context;
        private readonly IKafkaProducer<string, EdtUserProvisioning> kafkaProducer;

        public CommandHandler(
            IClock clock,
            IKeycloakAdministrationClient keycloakClient,
            ILogger<CommandHandler> logger,
            PidpConfiguration config,
            PidpDbContext context,
            IKafkaProducer<string, EdtUserProvisioning> kafkaProducer)
        {
            this.clock = clock;
            this.keycloakClient = keycloakClient;
            this.logger = logger;
            this.context = context;
            this.kafkaProducer = kafkaProducer;
            this.config = config;
        }

        public async Task<IDomainResult> HandleAsync(Command command)
        {
            var dto = await this.GetPidpUser(command);

            if (dto.AlreadyEnroled
                || dto.Email == null)
            {
                this.logger.LogDigitalEvidenceAccessRequestDenied();
                return DomainResult.Failed();
            }

            if (!await this.UpdateKeycloakUser(dto.UserId, command.AssignedRegions, command.ParticipantId))
            {
                
                return DomainResult.Failed();
            }

            using var trx = this.context.Database.BeginTransaction();

            try
            {
                var digitalEvidence = await this.SubmitDigitalEvidenceRequest(command); //save all trx at once for production(remove this and handle using idempotent)

                //publish accessRequest Event (Sending Events to the Outbox)

                var exportedEvent = this.AddOutbox(command, digitalEvidence, dto);

                await this.PublishAccessRequest(command, dto, digitalEvidence);

                await this.context.SaveChangesAsync();
                await trx.CommitAsync();


            }
            catch (Exception ex)
            {
                this.logger.LogDigitalEvidenceAccessTrxFailed(ex.Message.ToString());
                await trx.RollbackAsync();
                return DomainResult.Failed();
            }

            return DomainResult.Success();
        }

        private async Task<PartyDto> GetPidpUser(Command command)
        {
            return await this.context.Parties
                .Where(party => party.Id == command.PartyId)
                .Select(party => new PartyDto
                {
                    AlreadyEnroled = party.AccessRequests.Any(request => request.AccessTypeCode == AccessTypeCode.DigitalEvidence),
                    Cpn = party.Cpn,
                    Jpdid = party.Jpdid,
                    UserId = party.UserId,
                    Email = party.Email,
                    FirstName = party.FirstName,
                    LastName = party.LastName,
                    Phone = party.Phone
                })
                .SingleAsync();
        }

        private async Task PublishAccessRequest(Command command, PartyDto dto, Models.DigitalEvidence digitalEvidence)
        {
            await this.kafkaProducer.ProduceAsync(this.config.KafkaCluster.ProducerTopicName, $"{digitalEvidence.Id}", new EdtUserProvisioning
            {
                Key = $"{command.ParticipantId}",
                UserName = dto.Jpdid,
                Email = dto.Email,
                PhoneNumber = dto.Phone!,
                FullName = $"{dto.FirstName} {dto.LastName}",
                AccountType = "Saml",
                Role = "User",
                AssignedRegions = command.AssignedRegions
            });
        }

        private async Task<Models.DigitalEvidence> SubmitDigitalEvidenceRequest(Command command)
        {
            var digitalEvident = new Models.DigitalEvidence
            {
                PartyId = command.PartyId,
                Status = AccessRequestStatus.Pending,
                OrganizationType = command.OrganizationType.ToString(),
                OrganizationName = command.OrganizationName,
                ParticipantId = command.ParticipantId,
                AccessTypeCode = AccessTypeCode.DigitalEvidence,
                RequestedOn = this.clock.GetCurrentInstant(),
                AssignedRegions = command.AssignedRegions
            };
            this.context.DigitalEvidences.Add(digitalEvident);

            await this.context.SaveChangesAsync();
            return digitalEvident;
        }
        private Task<Models.OutBoxEvent.ExportedEvent> AddOutbox(Command command, Models.DigitalEvidence digitalEvidence, PartyDto dto)
        {
            var exportedEvent = this.context.ExportedEvents.Add(new Models.OutBoxEvent.ExportedEvent
            {
                EventId = digitalEvidence.Id,
                AggregateType = AccessTypeCode.DigitalEvidence.ToString(),
                AggregateId = $"{command.PartyId}",
                EventType = "Access Request Created",
                EventPayload = new EdtUserProvisioning
                {
                    Key = $"{command.ParticipantId}",
                    UserName = dto.Jpdid,
                    Email = dto.Email,
                    PhoneNumber = dto.Phone!,
                    FullName = $"{dto.FirstName} {dto.LastName}",
                    AccountType = "Saml",
                    Role = "User",
                    AssignedRegions = command.AssignedRegions
                }
            });
            return Task.FromResult(exportedEvent.Entity);
        }

        private async Task<bool> UpdateKeycloakUser(Guid userId, IEnumerable<AssignedRegion> assignedGroup, string partId)
        {
            if (!await this.keycloakClient.UpdateUser(userId, (user) => user.SetPartId(partId)))
            {
                return false;
            }
            foreach (var group in assignedGroup)
            {
                if (!await this.keycloakClient.AddGrouptoUser(userId, group.RegionName))
                {
                    return false;
                }
            }

            return true;
        }
    }
}



public static partial class DigitalEvidenceLoggingExtensions
{
    [LoggerMessage(1, LogLevel.Warning, "Digital Evidence Access Request denied due to the Party Record not meeting all prerequisites.")]
    public static partial void LogDigitalEvidenceAccessRequestDenied(this ILogger logger);
    [LoggerMessage(2, LogLevel.Warning, "Digital Evidence Access Request Transaction failed due to the Party Record not meeting all prerequisites.")]
    public static partial void LogDigitalEvidenceAccessTrxFailed(this ILogger logger, string ex);
}
