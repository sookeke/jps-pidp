namespace edt.service.ServiceEvents.UserAccountCreation.ConsumerRetry;

using Polly;
using Polly.Retry;

public class RetryPolicy
{

    public Dictionary<string,AsyncRetryPolicy<Task>> RetryTasks { get; } = new Dictionary<string, AsyncRetryPolicy<Task>>();

    public RetryPolicy(EdtServiceConfiguration config)
    {

        config.RetryPolicy.RetryTopics.ForEach(topic =>
        {
            Serilog.Log.Logger.Information("Adding retry topic task {0}", topic.ToString());
            var asyncRetryPolicy = Policy.HandleResult<Task>(
            res => !res.IsCompleted || res.Exception != null)
            .WaitAndRetryAsync(topic.RetryCount, retryAttempt => TimeSpan.FromMinutes(topic.DelayMinutes),
            onRetry: (response, delay, retryCount, context) => context["retrycount"] = retryCount);
            this.RetryTasks.Add(topic.TopicName, asyncRetryPolicy);

        });


    }
}
