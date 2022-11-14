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
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslOauthbearerTokenEndpointUrl = config.KafkaCluster.SaslOauthbearerTokenEndpointUrl,
            //SaslOauthbearerMethod = SaslOauthbearerMethod.Oidc,
            SaslOauthbearerScope = "oidc",
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.Https,
            SslCaLocation = config.KafkaCluster.SslCaLocation,
            SslCertificateLocation = config.KafkaCluster.SslCertificateLocation,
            //SslKeyLocation = config.KafkaCluster.SslKeyLocation
        };
        var producerConfig = new ProducerConfig()
        {
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslOauthbearerScope = "oidc",
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.Https,
            SslCaLocation = config.KafkaCluster.SslCaLocation,
            SslCertificateLocation = config.KafkaCluster.SslCertificateLocation,
            Acks = Acks.All,
            EnableIdempotence = true,
            ApiVersionFallbackMs = 0,
            BrokerVersionFallback = "0.10.0.0"
        };

        var consumerConfig = new ConsumerConfig(clientConfig)
        {
            GroupId = "accessrequest-consumer-group",
            EnableAutoCommit = true,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoOffsetStore = false,
            AutoCommitIntervalMs = 4000,
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl
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
