namespace Pidp.Kafka.Consumer;

using System.Net;
using Pidp.Kafka.Consumer.Model;
using Pidp.Kafka.Interfaces;

public class NotificationAckService : BackgroundService
{
    private readonly IKafkaConsumer<string, NotificationAckModel> consumer;

    private readonly PidpConfiguration config;
    public NotificationAckService(IKafkaConsumer<string, NotificationAckModel> kafkaConsumer, PidpConfiguration config)
    {
        this.consumer = kafkaConsumer;
        this.config = config;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await this.consumer.Consume(this.config.KafkaCluster.ConsumerTopicName, stoppingToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{(int)HttpStatusCode.InternalServerError} ConsumeFailedOnTopic - {this.config.KafkaCluster.ConsumerTopicName}, {ex}");
        }
    }

    public override void Dispose()
    {
        this.consumer.Close();
        this.consumer.Dispose();

        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
