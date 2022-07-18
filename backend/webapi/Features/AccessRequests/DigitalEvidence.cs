namespace Pidp.Features.AccessRequests;

using DomainResults.Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NodaTime;

using Pidp.Data;
using Pidp.Infrastructure.Auth;
using Pidp.Infrastructure.HttpClients.Keycloak;
using Pidp.Infrastructure.HttpClients.Mail;
using Pidp.Infrastructure.HttpClients.Plr;
using Pidp.Infrastructure.Services;
using Pidp.Models;
using Pidp.Models.Lookups;

public class DigitalEvidence
{
    public class Command : ICommand<IDomainResult>
    {
        public int PartyId { get; set; }
        public UserType UserType { get; set; } = UserType.None;
        public string PidNumber { get; set; } = string.Empty;
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
            this.RuleFor(x => x.UserType).NotEmpty();
            this.RuleFor(x => x.PartyId).GreaterThan(0);
        }
    }

    public class CommandHandler : ICommandHandler<Command, IDomainResult>
    {
        private readonly IClock clock;
        private readonly IEmailService emailService;
        private readonly IKeycloakAdministrationClient keycloakClient;
        private readonly ILogger logger;
        private readonly IPlrClient plrClient;
        private readonly PidpDbContext context;

        public CommandHandler(
            IClock clock,
            IEmailService emailService,
            IKeycloakAdministrationClient keycloakClient,
            ILogger<CommandHandler> logger,
            IPlrClient plrClient,
            PidpDbContext context)
        {
            this.clock = clock;
            this.emailService = emailService;
            this.keycloakClient = keycloakClient;
            this.logger = logger;
            this.plrClient = plrClient;
            this.context = context;
        }

        public async Task<IDomainResult> HandleAsync(Command command)
        {
            var dto = await this.context.Parties
                .Where(party => party.Id == command.PartyId)
                .Select(party => new
                {
                    AlreadyEnroled = party.AccessRequests.Any(request => request.AccessTypeCode == AccessTypeCode.DigitalEvidence),
                    party.PartyCertification!.Ipc,
                    party.Hpdid,
                    party.UserId,
                    party.Email
                })
                .SingleAsync();

            if (dto.AlreadyEnroled
                || dto.Email == null
                //|| dto.Hpdid == null
               /* || (await this.plrClient.GetRecordStatus(dto.Ipc))?.IsGoodStanding() != true*/)
            {
                this.logger.LogDigitalEvidenceAccessRequestDenied();
                return DomainResult.Failed();
            }
            switch (command.UserType)
            {
                case UserType.BCPS:
                    break;
                case UserType.Police:
                    break;
                case UserType.OutOfCustody:
                    //var outOfCustodyVerification = await this.ldapClient.HcimLoginAsync(command.UserType, command.PersonalIdenityficationNumber);
                    break;
                case UserType.Lawyer:
                    break;
                case UserType.None:
                    break;
                default:
                    break;
            }
            if (!await this.keycloakClient.AssignClientRole(dto.UserId, Resources.PidpApi, Roles.User))
            {
                return DomainResult.Failed();
            }

            //this.context.AccessRequests.Add(new AccessRequest
            //{
            //    PartyId = command.PartyId,
            //    AccessType = AccessType.DigitalEvidence,
            //    RequestedOn = this.clock.GetCurrentInstant()
            //});
            this.context.DigitalEvidences.Add(new Models.DigitalEvidence
            {
                PartyId = command.PartyId,
                UserType = command.UserType.ToString(),
                ParticipantId = command.PidNumber,
                AccessTypeCode = AccessTypeCode.DigitalEvidence,
                RequestedOn = this.clock.GetCurrentInstant()

            });

            await this.context.SaveChangesAsync();

            await this.SendConfirmationEmailAsync(dto.Email);

            return DomainResult.Success();
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
    }
}



public static partial class DigitalEvidenceLoggingExtensions
{
    [LoggerMessage(1, LogLevel.Warning, "Digital Evidence Access Request denied due to the Party Record not meeting all prerequisites.")]
    public static partial void LogDigitalEvidenceAccessRequestDenied(this ILogger logger);
}
