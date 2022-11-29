namespace edt.service.ServiceEvents.UserAccountCreation.ConsumerRetry;

public class UserProvisoningRetry
{
    public int RetryNumber { get; set; }
    public TimeSpan RetryDuration { get; set; }
}
