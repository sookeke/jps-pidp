namespace edt.service.ServiceEvents.UserAccountCreation.ConsumerRetry;

using System.Net;
using edt.service.Kafka.Interfaces;

public class ConsumerRetryService : BackgroundService
{
    private readonly IKafkaConsumer<string, UserProvisoningRetry> consumer;
    private readonly EdtServiceConfiguration config;
    public ConsumerRetryService(IKafkaConsumer<string, UserProvisoningRetry> consumer, EdtServiceConfiguration config)
    {
        this.consumer = consumer;
        this.config = config;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var retryTopics = new List<string>() { this.config.KafkaCluster.InitialRetryTopicName, this.config.KafkaCluster.MidRetryTopicName, this.config.KafkaCluster.FinalRetryTopicName };
            await this.consumer.RetryConsume(retryTopics, stoppingToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{(int)HttpStatusCode.InternalServerError} ConsumeFailedOnTopic - {this.config.KafkaCluster.ConsumerTopicName}, {ex}");
        }
    }

    public override void Dispose()
    {
        this.consumer.CloseRetry();
        this.consumer.DisposeRetry();

        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
