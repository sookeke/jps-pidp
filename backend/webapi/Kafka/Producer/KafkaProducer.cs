namespace Pidp.Kafka.Producer;

using Confluent.Kafka;
using Pidp.Kafka.Interfaces;
using IdentityModel.Client;
using System.Globalization;
using Serilog;

public class KafkaProducer<TKey, TValue> : IDisposable, IKafkaProducer<TKey, TValue> where TValue : class
{
    private readonly IProducer<TKey, TValue> producer;
    private const string EXPIRY_CLAIM = "exp";
    private const string SUBJECT_CLAIM = "sub";

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

            Log.Logger.Information("Producer getting token {0}", tokenEndpoint);


            var accessToken = await accessTokenClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                GrantType = "client_credentials"
            });
            var tokenTicks = GetTokenExpirationTime(accessToken.AccessToken);
            var subject = GetTokenSubject(accessToken.AccessToken);
            var tokenDate = DateTimeOffset.FromUnixTimeSeconds(tokenTicks);
            var timeSpan = new DateTime() - tokenDate;
            var ms = tokenDate.ToUnixTimeMilliseconds();
            Log.Logger.Information("Producer got token {0}", ms);

            client.OAuthBearerSetToken(accessToken.AccessToken, ms, subject);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex.Message);
            client.OAuthBearerSetTokenFailure(ex.ToString());
        }
    }
    private static long GetTokenExpirationTime(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);

        var tokenExp = jwtSecurityToken.Claims.First(claim => claim.Type.Equals(KafkaProducer<TKey, TValue>.EXPIRY_CLAIM, StringComparison.Ordinal)).Value;
        var ticks = long.Parse(tokenExp, CultureInfo.InvariantCulture);
        return ticks;
    }

    private static string GetTokenSubject(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        return jwtSecurityToken.Claims.First(claim => claim.Type.Equals(KafkaProducer<TKey, TValue>.SUBJECT_CLAIM, StringComparison.Ordinal)).Value;

    }
}
