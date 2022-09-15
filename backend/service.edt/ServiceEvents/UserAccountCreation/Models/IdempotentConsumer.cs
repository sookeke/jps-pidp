namespace edt.service.ServiceEvents.UserAccountCreation.Models;
public class IdempotentConsumer
{
    public string MessageId { get; set; } = string.Empty;
    public string Consumer { get; set; } = string.Empty;
}

