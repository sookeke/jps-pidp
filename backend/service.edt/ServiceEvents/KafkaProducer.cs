namespace edt.service.ServiceEvents;

using Confluent.Kafka;
using edt.service.Kafka.Interfaces;
using EdtService.Kafka;
using IdentityModel.Client;
using System.Globalization;

public class KafkaProducer<TKey, TValue> : KafkaOauthTokenRefreshHandler, IDisposable, IKafkaProducer<TKey, TValue> where TValue : class
{
    private readonly IProducer<TKey, TValue> producer;
    /// <summary>
    /// for production use of sasl/oauthbearer
    /// implement authentication callbackhandler for token retrival
    /// https://docs.confluent.io/platform/current/kafka/authentication_sasl/authentication_sasl_oauth.html#production-use-of-sasl-oauthbearer
    /// https://techcommunity.microsoft.com/t5/fasttrack-for-azure/event-hub-kafka-endpoint-azure-ad-authentication-using-c/ba-p/2586185
    /// https://github.com/Azure/azure-event-hubs-for-kafka/issues/97
    /// https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/test/Confluent.Kafka.IntegrationTests/Tests/OauthBearerToken_PublishConsume.cs
    /// </summary>
    /// <param name="config"></param>
    public KafkaProducer(ProducerConfig config) => this.producer = new ProducerBuilder<TKey, TValue>(config).SetOAuthBearerTokenRefreshHandler(OauthTokenRefreshCallback).SetValueSerializer(new KafkaSerializer<TValue>()).Build();
    public async Task ProduceAsync(string topic, TKey key, TValue value) => await this.producer.ProduceAsync(topic, new Message<TKey, TValue> { Key = key, Value = value });
    public void Dispose()
    {
        this.producer.Flush();
        this.producer.Dispose();
        GC.SuppressFinalize(this);
    }

    private static async void OauthTokenRefreshCallback(IClient client, string config)
    {
        try
        {
            var clusterConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();
            var tokenEndpoint = clusterConfig.GetValue<string>("KafkaCluster:SaslOauthbearerTokenEndpointUrl");
            var clientId = clusterConfig.GetValue<string>("KafkaCluster:SaslOauthbearerProducerClientId");
            var clientSecret = clusterConfig.GetValue<string>("KafkaCluster:SaslOauthbearerProducerClientSecret");
            var accessTokenClient = new HttpClient();
            var accessToken = await accessTokenClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                GrantType = "client_credentials"
            });
            var tokenTicks = GetTokenExpirationTime(accessToken.AccessToken);
            var tokenDate = DateTimeOffset.FromUnixTimeSeconds(tokenTicks);

            client.OAuthBearerSetToken(accessToken.AccessToken, tokenDate.ToUnixTimeMilliseconds(), null);
        }
        catch (Exception ex)
        {
            client.OAuthBearerSetTokenFailure(ex.ToString());
        }
    }
    private static long GetTokenExpirationTime(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        var tokenExp = jwtSecurityToken.Claims.First(claim => claim.Type.Equals("exp", StringComparison.Ordinal)).Value;
        var ticks = long.Parse(tokenExp, CultureInfo.InvariantCulture);
        return ticks;
    }
}

