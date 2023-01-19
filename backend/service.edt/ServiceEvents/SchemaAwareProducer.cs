namespace edt.service.ServiceEvents.UserAccountCreation.Handler;

using System.Diagnostics;
using Chr.Avro.Confluent;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using edt.service.Infrastructure.Telemetry;
using edt.service.Kafka.Interfaces;
using edt.service.Kafka.Model;
using Serilog;

/// <summary>
/// This producer will validate against the schema registry
/// </summary>
public class SchemaAwareProducer
{
    private readonly IKafkaProducer<string, UserModificationEvent> producer;
    private readonly ProducerConfig producerConfig;
    private ProducerBuilder<string, UserModificationEvent> builder;
    private readonly EdtServiceConfiguration serviceConfiguration;
    public SchemaAwareProducer(
              ProducerConfig producerConfig,
      IKafkaProducer<string, UserModificationEvent> producer,
       EdtServiceConfiguration serviceConfiguration
        )
    {
        this.producer = producer;
        this.producerConfig = producerConfig;
        this.serviceConfiguration = serviceConfiguration;
        this.builder = new ProducerBuilder<string, UserModificationEvent>(this.producerConfig);
    }



    public async Task<bool> ProduceAsync(string userModificationTopicName, string key, UserModificationEvent result)
    {

        var message = new Message<string, UserModificationEvent> { Key = key, Value = result };


        var success = false;
        var avroSerializerConfig = new AvroSerializerConfig
        {
            BufferBytes = 100
        };

        var schemaRegistryConfig = new SchemaRegistryConfig
        {
            Url = this.serviceConfiguration.SchemaRegistry.Url,
            BasicAuthUserInfo = this.serviceConfiguration.SchemaRegistry.ClientId + ":" + this.serviceConfiguration.SchemaRegistry.ClientSecret
        };

        using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);

        await Task.WhenAll(this.builder.SetAvroKeySerializer(schemaRegistry, $"{nameof(UserModificationEvent)}-key", registerAutomatically: AutomaticRegistrationBehavior.Always),
        this.builder.SetAvroValueSerializer(schemaRegistry, $"{nameof(UserModificationEvent)}-value", AutomaticRegistrationBehavior.Always));

        var registryAwareProducer = this.builder.Build();
        var activity = Diagnostics.Producer.Start(userModificationTopicName, message);

        try
        {
            await registryAwareProducer.ProduceAsync(userModificationTopicName, new Message<string, UserModificationEvent> { Key = key, Value = result }).ContinueWith(task =>
            {
                if (!task.IsFaulted)
                {
                    Log.Logger.Information("Published to {0}", userModificationTopicName);
                    success = true;
                }
                else
                {
                    Log.Logger.Error("Failed to produce message {0}", task.Exception);
                }
            });
        }
        finally
        {
            activity?.Stop();
        }


        return success;
    }
}
