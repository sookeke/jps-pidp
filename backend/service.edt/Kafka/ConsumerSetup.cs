namespace edt.service.Kafka;

using Confluent.Kafka;
using edt.service.HttpClients.Services.EdtCore;
using edt.service.Kafka.Interfaces;
using edt.service.ServiceEvents;
using edt.service.ServiceEvents.UserAccountCreation;
using edt.service.ServiceEvents.UserAccountCreation.Handler;
using EdtService.Extensions;

public static class ConsumerSetup
{
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, EdtServiceConfiguration config)
    {
        //Configuration = configuration;
        services.ThrowIfNull(nameof(services));
        config.ThrowIfNull(nameof(config));

        var clientConfig = new ClientConfig()
        {
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            SaslMechanism = SaslMechanism.Plain,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslUsername = config.KafkaCluster.ClientId,
            SaslPassword = config.KafkaCluster.ClientSecret,
        };
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            Acks = Acks.All,
            SaslMechanism = SaslMechanism.Plain,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslUsername = config.KafkaCluster.ClientId,
            SaslPassword = config.KafkaCluster.ClientSecret,
            EnableIdempotence = true
        };

        var consumerConfig = new ConsumerConfig(clientConfig)
        {
            GroupId = "Dems-Consumer-Group",
            EnableAutoCommit = true,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            EnableAutoOffsetStore = false,
            AutoCommitIntervalMs = 4000,
            SaslMechanism = SaslMechanism.Plain,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslUsername = config.KafkaCluster.ClientId,
            SaslPassword = config.KafkaCluster.ClientSecret
        };
        //var producerConfig = new ProducerConfig(clientConfig);
        services.AddSingleton(consumerConfig);
        services.AddSingleton(producerConfig);

        services.AddSingleton(typeof(IKafkaProducer<,>), typeof(KafkaProducer<,>));

        //services.AddSingleton(consumerConfig);

        services.AddScoped<IKafkaHandler<string, EdtUserProvisioningModel>, UserProvisioningHandler>();
        services.AddSingleton(typeof(IKafkaConsumer<,>), typeof(KafkaConsumer<,>));
        services.AddHostedService<EdtServiceConsumer>();

        return services;
    }
}
