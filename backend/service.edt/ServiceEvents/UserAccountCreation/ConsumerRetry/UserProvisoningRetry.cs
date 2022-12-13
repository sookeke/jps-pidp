namespace edt.service.ServiceEvents.UserAccountCreation.ConsumerRetry;

using NodaTime;

public class UserProvisoningRetry
{
    public int RetryNumber { get; set; }
    public TimeSpan RetryDuration { get; set; }
    public int TopicOrder { get; set; }

    public string? Created { get; set; }
}
