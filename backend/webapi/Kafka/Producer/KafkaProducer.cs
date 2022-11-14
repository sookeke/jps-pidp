namespace Pidp.Kafka.Producer;

using Confluent.Kafka;
using Pidp.Kafka.Interfaces;
using IdentityModel.Client;
using System.Globalization;

public class KafkaProducer<TKey, TValue> : IDisposable, IKafkaProducer<TKey, TValue> where TValue : class
{
    private readonly IProducer<TKey, TValue> producer;
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
