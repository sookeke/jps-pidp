namespace edt.service.ServiceEvents.UserAccountCreation.ConsumerRetry;

using Polly;
using Polly.Retry;

public class RetryPolicy
{
    private readonly EdtServiceConfiguration config;
    public AsyncRetryPolicy<Task> ImmediateConsumerRetry { get; }
    public AsyncRetryPolicy<Task> WaitForConsumerRetry { get; }
    public AsyncRetryPolicy<Task> FinalWaitForConsumerRetry { get; }
    public RetryPolicy(EdtServiceConfiguration config)
    {
        this.config = config;
        this.ImmediateConsumerRetry = Policy.HandleResult<Task>(
            res => !res.IsCompleted || res.Exception != null)
            .WaitAndRetryAsync(this.config.RetryPolicy.InitialRetryTopicName.RetryCount, retryAttempt => TimeSpan.FromMinutes(this.config.RetryPolicy.InitialRetryTopicName.WaitAfterInMins),
            onRetry: (response, delay, retryCount, context) => context["retrycount"] = retryCount);
        this.WaitForConsumerRetry = Policy.HandleResult<Task>(
            res => res.Status != TaskStatus.RanToCompletion && res.Exception != null)
          .WaitAndRetryAsync(this.config.RetryPolicy.MidRetryTopicName.RetryCount, retryAttempt => TimeSpan.FromMinutes(this.config.RetryPolicy.MidRetryTopicName.WaitAfterInMins), //retry nth times every nth minutes
            onRetry: (response, delay, retryCount, context) => context["retrycount"] = retryCount);
        this.FinalWaitForConsumerRetry = Policy.HandleResult<Task>(
            res => res.Status != TaskStatus.RanToCompletion && res.Exception != null)
                .WaitAndRetryAsync(this.config.RetryPolicy.FinalRetryTopicName.RetryCount, retryAttempt => TimeSpan.FromMinutes(this.config.RetryPolicy.FinalRetryTopicName.WaitAfterInMins),
            onRetry: (response, delay, retryCount, context) => context["retrycount"] = retryCount);
    }
}
