namespace Pidp.Kafka.Consumer;

using Confluent.Kafka;
using Pidp.Kafka.Interfaces;
using IdentityModel.Client;
using System.Globalization;
using Serilog;

public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue> where TValue : class
{
    private const string EXPIRY_CLAIM = "exp";
    private const string SUBJECT_CLAIM = "sub";
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

        Log.Logger.Information("PIDP Starting consumer for topic {0}", topic);

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

        Log.Logger.Information("Start consuming from {0}", this.topic);
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
                Log.Logger.Information("Consumer error ontopic {0} [{1}]", this.topic, e.Message);

                if (e.Error.IsFatal)
                {
                    break;
                }
            }
            catch (Exception e)
            {
                Log.Logger.Information("General consumer error on topic {0} [{1}]", this.topic, e.Message);

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

            var tokenEndpoint = Environment.GetEnvironmentVariable("KafkaCluster__SaslOauthbearerTokenEndpointUrl");
            var clientId = Environment.GetEnvironmentVariable("KafkaCluster__SaslOauthbearerConsumerClientId");
            var clientSecret = Environment.GetEnvironmentVariable("KafkaCluster__SaslOauthbearerConsumerClientSecret");

            clientSecret ??= clusterConfig.GetValue<string>("KafkaCluster:SaslOauthbearerConsumerClientSecret");
            clientId ??= clusterConfig.GetValue<string>("KafkaCluster:SaslOauthbearerConsumerClientId");
            tokenEndpoint ??= clusterConfig.GetValue<string>("KafkaCluster:SaslOauthbearerTokenEndpointUrl");
            Log.Logger.Debug("Pidp Kafka Consumer getting token {0} {1}", tokenEndpoint, clientId);
            var accessTokenClient = new HttpClient();


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
            Log.Logger.Debug("Consumer got token {0}", ms);

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

        var tokenExp = jwtSecurityToken.Claims.First(claim => claim.Type.Equals(KafkaConsumer<TKey, TValue>.EXPIRY_CLAIM, StringComparison.Ordinal)).Value;
        var ticks = long.Parse(tokenExp, CultureInfo.InvariantCulture);
        return ticks;
    }

    private static string GetTokenSubject(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        return jwtSecurityToken.Claims.First(claim => claim.Type.Equals(KafkaConsumer<TKey, TValue>.SUBJECT_CLAIM, StringComparison.Ordinal)).Value;

    }
}
