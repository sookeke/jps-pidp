namespace edt.service.ServiceEvents.UserAccountCreation.Handler;

using Chr.Avro.Confluent;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using edt.service.Kafka.Interfaces;
using edt.service.Kafka.Model;
using Serilog;

public class SchemaAwareProducer
{
    private readonly IKafkaProducer<string, UserModificationEvent> producer;
    private readonly ProducerConfig producerConfig;
    private ProducerBuilder<string, UserModificationEvent> builder;

    public SchemaAwareProducer(
              ProducerConfig producerConfig,
      IKafkaProducer<string, UserModificationEvent> producer
        )
    {
        this.producer = producer;
        this.producerConfig = producerConfig;
    }



    public async Task<bool> ProduceAsync(string userModificationTopicName, string key, UserModificationEvent result)
    {

        var success = false;
        var avroSerializerConfig = new AvroSerializerConfig
        {
            BufferBytes = 100
        };

        var schemaRegistryConfig = new SchemaRegistryConfig
        {
            Url = "http://pidp-kafka-apicurioregistry-pgsql.5b7aa5-dev.router-default.apps.silver.devops.gov.bc.ca/apis/ccompat/v6",
            BasicAuthUserInfo = "registry-client-api:hPYOGgxHb8W1Wd4BAfkrvBF93IaBHI0d"
        };

        using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);
        this.builder = new ProducerBuilder<string, UserModificationEvent>(this.producerConfig);
        await Task.WhenAll(this.builder.SetAvroKeySerializer(schemaRegistry, $"{nameof(UserModificationEvent)}-key", registerAutomatically: AutomaticRegistrationBehavior.Always),
        this.builder.SetAvroValueSerializer(schemaRegistry, $"{nameof(UserModificationEvent)}-value", AutomaticRegistrationBehavior.Always));

        var registryAwareProducer = this.builder.Build();


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


        return success;
    }
}
