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
            BootstrapServers = config.KafkaCluster.BootstrapServers,
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslOauthbearerTokenEndpointUrl = config.KafkaCluster.SaslOauthbearerTokenEndpointUrl,
            SaslOauthbearerMethod = SaslOauthbearerMethod.Oidc,
            SaslOauthbearerScope = config.KafkaCluster.Scope,
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.Https,
            SslCaLocation = config.KafkaCluster.SslCaLocation,
            SslCertificateLocation = config.KafkaCluster.SslCertificateLocation,
            SslKeyLocation = config.KafkaCluster.SslKeyLocation
        };
        var producerConfig = new ProducerConfig()
        {
            BootstrapServers = config.KafkaCluster.BootstrapServers,
            Acks = Acks.All,
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslOauthbearerTokenEndpointUrl = config.KafkaCluster.SaslOauthbearerTokenEndpointUrl,
            SaslOauthbearerMethod = SaslOauthbearerMethod.Oidc,
            SaslOauthbearerScope = config.KafkaCluster.Scope,
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.Https,
            SslCaLocation = config.KafkaCluster.SslCaLocation,
            SaslOauthbearerClientId = config.KafkaCluster.SaslOauthbearerProducerClientId,
            SaslOauthbearerClientSecret = config.KafkaCluster.SaslOauthbearerProducerClientSecret,
            SslCertificateLocation = config.KafkaCluster.SslCertificateLocation,
            SslKeyLocation = config.KafkaCluster.SslKeyLocation,
            EnableIdempotence = true,
            RetryBackoffMs = 1000,
            MessageSendMaxRetries = 3
        };

        var consumerConfig = new ConsumerConfig(clientConfig)
        {
            GroupId = config.KafkaCluster.ConsumerGroupId,
            EnableAutoCommit = true,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoOffsetStore = false,
            AutoCommitIntervalMs = 4000,
            BootstrapServers = config.KafkaCluster.BootstrapServers,
            SaslOauthbearerClientId = config.KafkaCluster.SaslOauthbearerConsumerClientId,
            SaslOauthbearerClientSecret = config.KafkaCluster.SaslOauthbearerConsumerClientSecret,
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl
        };
        services.AddSingleton(consumerConfig);
        services.AddSingleton(producerConfig);

        services.AddSingleton(typeof(IKafkaProducer<,>), typeof(KafkaProducer<,>));


        services.AddScoped<IKafkaHandler<string, EdtUserProvisioningModel>, UserProvisioningHandler>();
        services.AddSingleton(typeof(IKafkaConsumer<,>), typeof(KafkaConsumer<,>));
        services.AddHostedService<EdtServiceConsumer>();

        return services;
    }
}
