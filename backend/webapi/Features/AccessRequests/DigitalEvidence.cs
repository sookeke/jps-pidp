namespace Pidp.Features.AccessRequests;
using DomainResults.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;

using Pidp.Data;
using Pidp.Infrastructure.Auth;
using Pidp.Infrastructure.HttpClients.Keycloak;
using Pidp.Infrastructure.HttpClients.Mail;
using Pidp.Infrastructure.Services;
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
            this.RuleFor(x => x.PartyId).GreaterThan(0);
        }
    }

    public class CommandHandler : ICommandHandler<Command, IDomainResult>
    {
        private readonly IClock clock;
        private readonly IEmailService emailService;
        private readonly IKeycloakAdministrationClient keycloakClient;
        private readonly ILogger logger;
        private readonly PidpConfiguration config;
        //private readonly IUserTypeService userTypeService;
        private readonly PidpDbContext context;
        private readonly IKafkaProducer<string, EdtUserProvisioning> kafkaProducer;

        public CommandHandler(
            IClock clock,
            IEmailService emailService,
            IKeycloakAdministrationClient keycloakClient,
            ILogger<CommandHandler> logger,
            //IUserTypeService userTypeService,
            PidpConfiguration config,
            PidpDbContext context,
            IKafkaProducer<string, EdtUserProvisioning> kafkaProducer)
        {
            this.clock = clock;
            this.emailService = emailService;
            this.keycloakClient = keycloakClient;
            this.logger = logger;
            //this.userTypeService = userTypeService;
            this.context = context;
            this.kafkaProducer = kafkaProducer;
            this.config = config;
        }

        public async Task<IDomainResult> HandleAsync(Command command)
        {
            var dto = await this.context.Parties
                .Where(party => party.Id == command.PartyId)
                .Select(party => new
                {
                    AlreadyEnroled = party.AccessRequests.Any(request => request.AccessTypeCode == AccessTypeCode.DigitalEvidence),
                    party.Cpn,
                    party.Jpdid,
                    party.UserId,
                    party.Email,
                    party.FirstName,
                    party.LastName,
                    party.Phone
                })
                .SingleAsync();

            //var orgType = await this.userTypeService.GetOrgUserType(command.PartyId);
            //orgType!.ThrowIfNull(nameof(orgType));
            //var userType = orgType!.Values.Select(n => n.Keys.Skip(1).FirstOrDefault()).First();

            if (dto.AlreadyEnroled
                || dto.Email == null
               //|| dto.Hpdid == null
               /* || (await this.plrClient.GetRecordStatus(dto.Ipc))?.IsGoodStanding() != true*/) //check justin api for user standing
            {
                this.logger.LogDigitalEvidenceAccessRequestDenied();
                return DomainResult.Failed();
            }
            //switch (command.UserType)
            //{
            //    case UserType.BCPS:
            //        break;
            //    case UserType.Police:
            //        break;
            //    case UserType.OutOfCustody:
            //        //var outOfCustodyVerification = await this.ldapClient.HcimLoginAsync(command.UserType, command.PersonalIdenityficationNumber);
            //        break;
            //    case UserType.Lawyer:
            //        break;
            //    case UserType.None:
            //        break;
            //    default:
            //        break;
            //}
            //if (!await this.keycloakClient.AssignClientRole(dto.UserId, Clients.PidpApi, Roles.User))
            //{
            //    return DomainResult.Failed();
            //}

            //if (!await this.UpdateKeycloakUser(dto.UserId, Clients.PidpApi, Roles.User, command.ParticipantId))
            //{
            //    return DomainResult.Failed();
            //}

            using var trx = this.context.Database.BeginTransaction();

            try
            {
                var digitalEvenident = new Models.DigitalEvidence
                {
                    PartyId = command.PartyId,
                    Status = AccessRequestStatus.Pending,
                    OrganizationType = command.OrganizationType.ToString(),
                    OrganizationName = command.OrganizationName,
                    ParticipantId = command.ParticipantId,
                    AccessTypeCode = AccessTypeCode.DigitalEvidence,
                    RequestedOn = this.clock.GetCurrentInstant()
                };
                this.context.DigitalEvidences.Add(digitalEvenident);

                await this.context.SaveChangesAsync(); //save all trx at once for production(remove this and handle using idempotent)
                //publish accessRequest Event (Sending Events to the Outbox)

                this.context.ExportedEvents.Add(new Models.OutBoxEvent.ExportedEvent
                {
                    EventId = digitalEvenident.Id,
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
                        Role = "User"
                    }
                });

                await this.kafkaProducer.ProduceAsync(this.config.KafkaCluster.ProducerTopicName, $"{digitalEvenident.Id}", new EdtUserProvisioning
                {
                    Key = $"{command.ParticipantId}",
                    UserName = dto.Jpdid,
                    Email = dto.Email,
                    PhoneNumber = dto.Phone!,
                    FullName = $"{dto.FirstName} {dto.LastName}",
                    AccountType = "Saml",
                    Role = "User"
                });

                await this.context.SaveChangesAsync();
                await trx.CommitAsync();
            }
            catch (Exception)
            {
                //this.logger.LogTrace(ex.Message);
                await trx.RollbackAsync();
                return DomainResult.Failed();
            }


            //await this.SendConfirmationEmailAsync(dto.Email);

            return DomainResult.Success();
        }

        private async Task<bool> UpdateKeycloakUser(Guid userId, string client, string role, string partId)
        {
            if (!await this.keycloakClient.UpdateUser(userId, (user) => user.SetPartId(partId)))
            {
                return false;
            }

            if (!await this.keycloakClient.AssignClientRole(userId, client, role))
            {
                return false;
            }

            return true;
        }

        private async Task SendConfirmationEmailAsync(string partyEmail)
        {
            // TODO email text
            var email = new Email(
                from: EmailService.PidpEmail,
                to: partyEmail,
                subject: "Digital Evidence Management System Enrolment Confirmation",
                body: $"Digital Evidence Management System Enrolment Confirmation"
            );
            await this.emailService.SendAsync(email);
        }
        //private static bool IsValidJustinUser(JustinUser justinUser, Party party) => false;
    }
}



public static partial class DigitalEvidenceLoggingExtensions
{
    [LoggerMessage(1, LogLevel.Warning, "Digital Evidence Access Request denied due to the Party Record not meeting all prerequisites.")]
    public static partial void LogDigitalEvidenceAccessRequestDenied(this ILogger logger);
}
