namespace Pidp.Features.AccessRequests;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
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
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

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
        private readonly IKafkaProducer<string, Notification> kafkaNotificationProducer;


        public CommandHandler(
            IClock clock,
            IKeycloakAdministrationClient keycloakClient,
            ILogger<CommandHandler> logger,
            PidpConfiguration config,
            PidpDbContext context,
            IKafkaProducer<string, EdtUserProvisioning> kafkaProducer,
            IKafkaProducer<string, Notification> kafkaNotificationProducer)
        {
            this.clock = clock;
            this.keycloakClient = keycloakClient;
            this.logger = logger;
            this.context = context;
            this.kafkaProducer = kafkaProducer;
            this.config = config;
            this.kafkaNotificationProducer = kafkaNotificationProducer;
        }

        public async Task<IDomainResult> HandleAsync(Command command)
        {

            using (var activity = new Activity("DigitalEvidence Request").Start())
            {

                var traceId = Tracer.CurrentSpan.Context.TraceId;
                Serilog.Log.Logger.Information("DigitalEvidence Request {0} {1}", command.ParticipantId, traceId);

                Activity.Current?.AddTag("digitalevidence.party.id", command.PartyId);


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
                    var key = Guid.NewGuid().ToString();
                    Serilog.Log.Logger.Information("Sending submission message for {0} to {1}", command.ParticipantId, dto.Email);
                    if (digitalEvidence != null)
                    {

                        // send notification to user of sumission
                        await this.kafkaNotificationProducer.ProduceAsync(this.config.KafkaCluster.NotificationTopicName, key: key, new Notification
                        {
                            To = dto.Email,
                            From = "jpsprovideridentityportal@gov.bc.ca",
                            FirstName = dto.FirstName,
                            Subject = "Digital Evidence Management System Enrollment Request",
                            MsgBody = MsgBodySubmissionReceived(dto.FirstName),
                            PartyId = command.ParticipantId!,
                            Tag = key
                        });
                    }

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

        }

        private static string MsgBodySubmissionReceived(string? firstName)
        {
            var msgBody = string.Format(CultureInfo.CurrentCulture, @"<html>
            <head>
                <title>Digital Evidence Management System Enrollment Notification</title>
            </head>
                <body> 
                <img src='https://drive.google.com/uc?export=view&id=16JU6XoVz5FvFUXXWCN10JvN-9EEeuEmr' width='' height='50'/><br/><br/>
    <div style='border-top: 3px solid #22BCE5'><span style = 'font-family: Arial; font-size: 10pt' >
<br/> Hello {0},<br/><br/>Your BCPS DEMS access request has been received.<br/>
We will notify you when your account has been created<p/>{1}<p/>
<div style='border-top: 3px solid #22BCE5'>
                </span></div></body></html> ",
                    firstName, GetSupportMessage());
            return msgBody;
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
            Serilog.Log.Logger.Information("Adding message to topic {0} {1}", this.config.KafkaCluster.ProducerTopicName, command.ParticipantId);
            await this.kafkaProducer.ProduceAsync(this.config.KafkaCluster.ProducerTopicName, $"{digitalEvidence.Id}", new EdtUserProvisioning
            {
                Key = $"{command.ParticipantId}",
                UserName = dto.Jpdid,
                Email = dto.Email,
                PhoneNumber = dto.Phone!,
                FullName = $"{dto.FirstName} {dto.LastName}",
                AccountType = "Saml",
                Role = "User",
                AssignedRegions = command.AssignedRegions,
                AccessRequestId = digitalEvidence.Id
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

        private static string GetSupportMessage() => "<p/>If you require any assistance, please contact <a href = \"mailto:bcps.disclosure.support@gov.bc.ca\">bcps.disclosure.support@gov.bc.ca</a><p/><p/>Thank you,<br/>BCPS DEMS Support<p/>";


        private async Task<bool> UpdateKeycloakUser(Guid userId, IEnumerable<AssignedRegion> assignedGroup, string partId)
        {
            if (!await this.keycloakClient.UpdateUser(userId, (user) => user.SetPartId(partId)))
            {
                Serilog.Log.Logger.Error("Failed to set user {0} partId in keycloak", partId);

                return false;
            }
            foreach (var group in assignedGroup)
            {
                if (!await this.keycloakClient.AddGrouptoUser(userId, group.RegionName))
                {
                    Serilog.Log.Logger.Error("Failed to add user {0} group {1} to keycloak", partId, group.RegionName);
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
