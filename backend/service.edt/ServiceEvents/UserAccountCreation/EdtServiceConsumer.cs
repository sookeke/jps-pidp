namespace edt.service.ServiceEvents.UserAccountCreation;

using System.Net;
using edt.service;
using edt.service.HttpClients.Services.EdtCore;
using edt.service.Kafka.Interfaces;

public class EdtServiceConsumer : BackgroundService
{
    private readonly IKafkaConsumer<string, EdtUserProvisioningModel> consumer;

    private readonly EdtServiceConfiguration config;
    public EdtServiceConsumer(IKafkaConsumer<string, EdtUserProvisioningModel> kafkaConsumer, EdtServiceConfiguration config)
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

