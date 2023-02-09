namespace edt.service.ServiceEvents.UserAccountCreation.ConsumerRetry;

using System.Net;
using edt.service.HttpClients.Services.EdtCore;
using edt.service.Kafka.Interfaces;

public class ConsumerRetryService : BackgroundService
{
    private readonly IKafkaConsumer<string, EdtUserProvisioningModel> consumer;
    private readonly EdtServiceConfiguration config;
    public ConsumerRetryService(IKafkaConsumer<string, EdtUserProvisioningModel> consumer, EdtServiceConfiguration config)
    {
        this.consumer = consumer;
        this.config = config;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await this.consumer.RetryConsume(this.config.RetryPolicy.RetryTopics, stoppingToken);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error($"{(int)HttpStatusCode.InternalServerError} ConsumeFailedOnTopic - {this.config.KafkaCluster.ConsumerTopicName}, {ex}");
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
