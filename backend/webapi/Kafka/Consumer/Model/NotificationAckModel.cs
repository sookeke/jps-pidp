namespace Pidp.Kafka.Consumer.Model;

public class NotificationAckModel
{
    public string NotificationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PartId { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string Consumer { get; set; } = string.Empty;
    public int AccessRequestId { get; set; }
}
