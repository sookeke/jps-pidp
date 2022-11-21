namespace Pidp.Kafka.Consumer;

using Confluent.Kafka;
using Pidp.Kafka.Interfaces;
using IdentityModel.Client;
using System.Globalization;

public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue> where TValue : class
{
    private readonly ConsumerConfig config;
    private IKafkaHandler<TKey, TValue> handler;
    private IConsumer<TKey, TValue> consumer;
    private string topic;

    private readonly IServiceScopeFactory serviceScopeFactory;

    public KafkaConsumer(ConsumerConfig config, IServiceScopeFactory serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.config = config;
    }

    public async Task Consume(string topic, CancellationToken stoppingToken)
    {
        using var scope = this.serviceScopeFactory.CreateScope();

        this.handler = scope.ServiceProvider.GetRequiredService<IKafkaHandler<TKey, TValue>>();
        this.consumer = new ConsumerBuilder<TKey, TValue>(this.config).SetOAuthBearerTokenRefreshHandler(OauthTokenRefreshCallback).SetValueDeserializer(new KafkaDeserializer<TValue>()).Build();
        this.topic = topic;

        await Task.Run(() => this.StartConsumerLoop(stoppingToken), stoppingToken);
    }
    /// <summary>
    /// This will close the consumer, commit offsets and leave the group cleanly.
    /// </summary>
    public void Close() => this.consumer.Close();
    /// <summary>
    /// Releases all resources used by the current instance of the consumer
    /// </summary>
    public void Dispose() => this.consumer.Dispose();
    private async Task StartConsumerLoop(CancellationToken cancellationToken)
    {
        this.consumer.Subscribe(this.topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = this.consumer.Consume(cancellationToken);

                if (result != null)
                {
                    var consumerResult = await this.handler.HandleAsync(this.consumer.Name, result.Message.Key, result.Message.Value);

                    if (consumerResult.Status == TaskStatus.RanToCompletion && consumerResult.Exception == null)
                    {
                        this.consumer.Commit(result);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                // Consumer errors should generally be ignored (or logged) unless fatal.
                Console.WriteLine($"Consume error: {e.Error.Reason}");

                if (e.Error.IsFatal)
                {
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e}");
                break;
            }
        }
    }

    private static async void OauthTokenRefreshCallback(IClient client, string config)
    {
        try
        {
            var clusterConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();
            var tokenEndpoint = clusterConfig.GetValue<string>("KafkaCluster:SaslOauthbearerTokenEndpointUrl");
            var clientId = clusterConfig.GetValue<string>("KafkaCluster:SaslOauthbearerConsumerClientId");
            var clientSecret = clusterConfig.GetValue<string>("KafkaCluster:SaslOauthbearerConsumerClientSecret");
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
