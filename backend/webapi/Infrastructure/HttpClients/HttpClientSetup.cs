namespace Pidp.Infrastructure.HttpClients;

using System.Net;
using Confluent.Kafka;
using IdentityModel.Client;
using Pidp.Extensions;
using Pidp.Infrastructure.HttpClients.AddressAutocomplete;
using Pidp.Infrastructure.HttpClients.Jum;
using Pidp.Infrastructure.HttpClients.Keycloak;
using Pidp.Infrastructure.HttpClients.Ldap;
using Pidp.Infrastructure.HttpClients.Mail;
using Pidp.Infrastructure.HttpClients.Plr;
using Pidp.Kafka.Consumer;
using Pidp.Kafka.Consumer.Handler;
using Pidp.Kafka.Consumer.Model;
using Pidp.Kafka.Interfaces;
using Pidp.Kafka.Producer;

public static class HttpClientSetup
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services, PidpConfiguration config)
    {
        services.AddHttpClient<IAccessTokenClient, AccessTokenClient>();

        services.AddHttpClientWithBaseAddress<IAddressAutocompleteClient, AddressAutocompleteClient>(config.AddressAutocompleteClient.Url);

        services.AddHttpClientWithBaseAddress<IChesClient, ChesClient>(config.ChesClient.Url)
            .WithBearerToken(new ChesClientCredentials
            {
                Address = config.ChesClient.TokenUrl,
                ClientId = config.ChesClient.ClientId,
                ClientSecret = config.ChesClient.ClientSecret
            });

        services.AddHttpClientWithBaseAddress<ILdapClient, LdapClient>(config.LdapClient.Url);

        services.AddHttpClientWithBaseAddress<IJumClient, JumClient>(config.JumClient.Url);

        services.AddHttpClientWithBaseAddress<IKeycloakAdministrationClient, KeycloakAdministrationClient>(config.Keycloak.AdministrationUrl)
            .WithBearerToken(new KeycloakAdministrationClientCredentials
            {
                Address = config.Keycloak.TokenUrl,
                ClientId = config.Keycloak.AdministrationClientId,
                ClientSecret = config.Keycloak.AdministrationClientSecret
            });

        services.AddHttpClientWithBaseAddress<IPlrClient, PlrClient>(config.PlrClient.Url);

        services.AddTransient<ISmtpEmailClient, SmtpEmailClient>();

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

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config.KafkaCluster.BootstrapServers,
            Acks = Acks.All,
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslOauthbearerTokenEndpointUrl = config.KafkaCluster.SaslOauthbearerTokenEndpointUrl,
            SaslOauthbearerMethod = SaslOauthbearerMethod.Oidc,
            SaslOauthbearerScope = config.KafkaCluster.Scope,
            ClientId = Dns.GetHostName(),
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
            SaslOauthbearerClientId = config.KafkaCluster.SaslOauthbearerConsumerClientId,
            SaslOauthbearerClientSecret = config.KafkaCluster.SaslOauthbearerConsumerClientSecret,
            EnableAutoOffsetStore = false,
            ClientId = Dns.GetHostName(),
            AutoCommitIntervalMs = 4000,
            BootstrapServers = config.KafkaCluster.BootstrapServers,
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl
        };

        services.AddSingleton(consumerConfig);
        services.AddSingleton(producerConfig);
        services.AddTransient(typeof(IKafkaProducer<,>), typeof(KafkaProducer<,>));

        services.AddScoped<IKafkaHandler<string, NotificationAckModel>, NotificationAckHandler>();
        services.AddSingleton(typeof(IKafkaConsumer<,>), typeof(KafkaConsumer<,>));
        services.AddHostedService<NotificationAckService>();

        return services;
    }

    public static IHttpClientBuilder AddHttpClientWithBaseAddress<TClient, TImplementation>(this IServiceCollection services, string baseAddress)
        where TClient : class
        where TImplementation : class, TClient
        => services.AddHttpClient<TClient, TImplementation>(client => client.BaseAddress = new Uri(baseAddress.EnsureTrailingSlash()));

    public static IHttpClientBuilder WithBearerToken<T>(this IHttpClientBuilder builder, T credentials) where T : ClientCredentialsTokenRequest
    {
        builder.Services.AddSingleton(credentials)
            .AddTransient<BearerTokenHandler<T>>();

        builder.AddHttpMessageHandler<BearerTokenHandler<T>>();

        return builder;
    }
}
