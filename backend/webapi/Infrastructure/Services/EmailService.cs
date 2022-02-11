namespace Pidp.Infrastructure.Services;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Pidp;
using Pidp.Data;
using Pidp.Infrastructure.HttpClients.Mail;
using Pidp.Models;

public class EmailService : IEmailService
{
    private const string PidpEmail = "provideridentityportal@gov.bc.ca";
    private readonly PidpDbContext context;
    private readonly ILogger logger;
    private readonly PidpConfiguration config;
    private readonly IChesClient chesClient;
    private readonly ISmtpEmailClient smtpEmailClient;
    private readonly IClock clock;

    public EmailService(PidpDbContext context, ILogger<EmailService> logger, PidpConfiguration config, IChesClient chesClient, ISmtpEmailClient smtpEmailClient, IClock clock)
    {
        this.context = context;
        this.logger = logger;
        this.config = config;
        this.chesClient = chesClient;
        this.smtpEmailClient = smtpEmailClient;
        this.clock = clock;
    }

    public async Task SendSaEformsAccessRequestConfirmationAsync(int partyId)
    {
        var party = await this.context.Parties
            .Where(party => party.Id == partyId)
            .Select(party => new
            {
                party.FirstName,
                party.Email
            })
            .SingleOrDefaultAsync();

        if (party?.Email == null)
        {
            this.logger.LogError($"Could not send SA eForms access request confirmation.No email address found for partyId {partyId}");
            return;
        }

        var url = "https://www.eforms.phsahealth.ca/appdash";
        var link = $"<a href=\"{url}\" target=\"_blank\" rel=\"noopener noreferrer\">link</a>";
        var email = new Email(
            PidpEmail,
            party.Email,
            "SA eForms Enrolment Confirmation",
            $"Hi {party.FirstName},<br><br>You will need to visit this {link} each time you want to submit an SA eForm. It may be helpful to bookmark this {link} for future use."
        );
        await this.Send(email);
    }

    private async Task Send(Email email)
    {
        if (!PidpConfiguration.IsProduction())
        {
            email.Subject = $"THE FOLLOWING EMAIL IS A TEST: {email.Subject}";
        }

        if (this.config.ChesClient.Enabled && await this.chesClient.HealthCheckAsync())
        {
            var msgId = await this.chesClient.SendAsync(email);
            await this.CreateEmailLog(email, SendType.Ches, msgId);

            if (msgId != null)
            {
                return;
            }
        }

        // Fall back to SMTP client
        await this.smtpEmailClient.SendAsync(email);
        await this.CreateEmailLog(email, SendType.Smtp);
    }

    public async Task<int> UpdateEmailLogStatuses(int limit)
    {
        Expression<Func<EmailLog, bool>> predicate = log =>
            log.SendType == SendType.Ches
            && log.MsgId != null
            && log.LatestStatus != ChesStatus.Completed;

        var totalCount = await this.context.EmailLogs
            .Where(predicate)
            .CountAsync();

        var emailLogs = await this.context.EmailLogs
            .Where(predicate)
            .OrderBy(e => e.UpdateCount)
                .ThenBy(e => e.Modified)
            .Take(limit)
            .ToListAsync();

        foreach (var emailLog in emailLogs)
        {
            var status = await this.chesClient.GetStatusAsync(emailLog.MsgId!.Value);
            if (status != null && emailLog.LatestStatus != status)
            {
                emailLog.LatestStatus = status;
            }
            emailLog.UpdateCount++;
        }
        await this.context.SaveChangesAsync();

        return totalCount;
    }

    private async Task CreateEmailLog(Email email, string sendType, Guid? msgId = null)
    {
        this.context.EmailLogs.Add(new EmailLog(email, sendType, msgId, this.clock.GetCurrentInstant()));
        await this.context.SaveChangesAsync();
    }

    private static class SendType
    {
        public const string Ches = "CHES";
        public const string Smtp = "SMTP";
    }
}