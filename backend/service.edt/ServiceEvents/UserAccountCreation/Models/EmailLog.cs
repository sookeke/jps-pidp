namespace edt.service.ServiceEvents.UserAccountCreation.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

[Table(nameof(EmailLog))]
public class EmailLog : BaseAuditable
{
    [Key]
    public int Id { get; set; }

    public string SendType { get; set; } = string.Empty;

    public Guid? MsgId { get; set; }

    public string SentTo { get; set; } = string.Empty;

    public string Cc { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public Instant? DateSent { get; set; }

    public string? LatestStatus { get; set; }

    public string? StatusMessage { get; set; }

    public int UpdateCount { get; set; }

    public EmailLog() { }

}
