namespace edt.service.ServiceEvents;

using Confluent.Kafka;
using edt.service.HttpClients;
using edt.service.Kafka.Interfaces;
using EdtService.Kafka;
using IdentityModel.Client;

public class KafkaProducer<TKey, TValue> : IDisposable, IKafkaProducer<TKey, TValue> where TValue : class
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
    public KafkaProducer(ProducerConfig config) => this.producer = new ProducerBuilder<TKey, TValue>(config).SetValueSerializer(new KafkaSerializer<TValue>()).Build();
    public async Task ProduceAsync(string topic, TKey key, TValue value) => await this.producer.ProduceAsync(topic, new Message<TKey, TValue> { Key = key, Value = value });
    public void Dispose()
    {
        this.producer.Flush();
        this.producer.Dispose();
        GC.SuppressFinalize(this);
    }
    /// <summary>
    /// create a reusable method for get accesstoken and refreshhandler for kafka clients in production
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="config"></param>
    private async void TokenRefreshHandler(IProducer<TKey, TValue> producer, string config)
    {
        try
        {
            var accessTokenClient = new HttpClient();
            var accessToken = await accessTokenClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = "https://sso-dev-5b7aa5-dev.apps.silver.devops.gov.bc.ca/auth/realms/DEMSPOC/protocol/openid-connect/token",
                ClientId = "",
                ClientSecret = "",
            });
            producer.OAuthBearerSetToken(accessToken.AccessToken, accessToken.ExpiresIn, null);
        }
        catch (Exception ex)
        {
            producer.OAuthBearerSetTokenFailure(ex.ToString());
        }
    }
}
