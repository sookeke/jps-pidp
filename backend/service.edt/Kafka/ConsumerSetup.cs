namespace edt.service.Kafka;

using Confluent.Kafka;
using edt.service.HttpClients.Services.EdtCore;
using edt.service.Kafka.Interfaces;
using edt.service.ServiceEvents;
using edt.service.ServiceEvents.UserAccountCreation;
using edt.service.ServiceEvents.UserAccountCreation.Handler;
using EdtService.Extensions;
using Serilog;

public static class ConsumerSetup
{
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, EdtServiceConfiguration config)
    {
        //Configuration = configuration;
        services.ThrowIfNull(nameof(services));
        config.ThrowIfNull(nameof(config));

        Log.Logger.Information("### bootstrap [{0}]", config.KafkaCluster.BoostrapServers);
        Log.Logger.Information("### endpoint [{0}]", config.KafkaCluster.SaslOauthbearerTokenEndpointUrl);
        Log.Logger.Information("### ca loc [{0}]", config.KafkaCluster.SslCaLocation);
        Log.Logger.Information("### cert loc [{0}]", config.KafkaCluster.SslCertificateLocation);
        Log.Logger.Information("### key loc [{0}]", config.KafkaCluster.SslKeyLocation);
        Log.Logger.Information("### producer id [{0}]", config.KafkaCluster.SaslOauthbearerProducerClientId);
        Log.Logger.Information("### producer # [{0}]", config.KafkaCluster.SaslOauthbearerProducerClientSecret);
        Log.Logger.Information("### consumer id [{0}]", config.KafkaCluster.SaslOauthbearerConsumerClientId);
        Log.Logger.Information("### consumer # [{0}]", config.KafkaCluster.SaslOauthbearerConsumerClientSecret);

        Log.Logger.Information("### consumer topic # [{0}]", config.KafkaCluster.ConsumerTopicName);
        Log.Logger.Information("### producer topic # [{0}]", config.KafkaCluster.ProducerTopicName);


        var clientConfig = new ClientConfig()
        {
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslOauthbearerTokenEndpointUrl = config.KafkaCluster.SaslOauthbearerTokenEndpointUrl,
            SaslOauthbearerMethod = SaslOauthbearerMethod.Oidc,
            SaslOauthbearerScope = "oidc",
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.Https,
            SslCaLocation = config.KafkaCluster.SslCaLocation,
            SslCertificateLocation = config.KafkaCluster.SslCertificateLocation,
            SslKeyLocation = config.KafkaCluster.SslKeyLocation

        };

 

        var producerConfig = new ProducerConfig()
        {
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslOauthbearerScope = "oidc",
            SaslOauthbearerMethod = SaslOauthbearerMethod.Oidc,
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.Https,
            SslCaLocation = config.KafkaCluster.SslCaLocation,
            SaslOauthbearerTokenEndpointUrl = config.KafkaCluster.SaslOauthbearerTokenEndpointUrl,
            SslCertificateLocation = config.KafkaCluster.SslCertificateLocation,
            SaslOauthbearerClientId = config.KafkaCluster.SaslOauthbearerProducerClientId,
            SaslOauthbearerClientSecret = config.KafkaCluster.SaslOauthbearerProducerClientSecret,
            Acks = Acks.All,
            EnableIdempotence = true,
            ApiVersionFallbackMs = 0,
            BrokerVersionFallback = "0.10.0.0"
        };

        var consumerConfig_orig = new ConsumerConfig(clientConfig)
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

        var consumerConfig = new ConsumerConfig(clientConfig)
        {
            GroupId = "accessrequest-consumer-group-testing",
            EnableAutoCommit = true,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            SaslOauthbearerClientId = config.KafkaCluster.SaslOauthbearerConsumerClientId,
            SaslOauthbearerClientSecret = config.KafkaCluster.SaslOauthbearerConsumerClientSecret,
            SaslOauthbearerMethod = SaslOauthbearerMethod.Oidc,
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
