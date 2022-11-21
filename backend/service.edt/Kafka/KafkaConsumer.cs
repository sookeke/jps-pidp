namespace edt.service.Kafka;

using Confluent.Kafka;
using edt.service.Kafka.Interfaces;
using IdentityModel.Client;
using Serilog;
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
        //this.handler = handler;
        //this.consumer = consumer;
        //this.topic = topic;
    }
    /// <summary>
    /// for production use of sasl/oauthbearer
    /// implement authentication callbackhandler for token retrival and refresh
    /// https://docs.confluent.io/platform/current/kafka/authentication_sasl/authentication_sasl_oauth.html#production-use-of-sasl-oauthbearer
    /// https://techcommunity.microsoft.com/t5/fasttrack-for-azure/event-hub-kafka-endpoint-azure-ad-authentication-using-c/ba-p/2586185
    /// https://github.com/Azure/azure-event-hubs-for-kafka/issues/97
    /// https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/test/Confluent.Kafka.IntegrationTests/Tests/OauthBearerToken_PublishConsume.cs
    /// </summary>
    /// <param name="config"></param>

    public async Task Consume(string topic, CancellationToken stoppingToken)
    {
        using var scope = this.serviceScopeFactory.CreateScope();

        this.handler = scope.ServiceProvider.GetRequiredService<IKafkaHandler<TKey, TValue>>();
        //this.consumer = new ConsumerBuilder<TKey, TValue>(this.config).SetValueDeserializer(new KafkaDeserializer<TValue>()).Build();
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
        Log.Logger.Information("### Starting topic consumer for {0}", this.topic);



        this.consumer.Subscribe(this.topic);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = this.consumer.Consume(cancellationToken);
                if (result != null)
                {
                    Log.Logger.Information("### Got message key {0} [{1}]", result.Message.Key, result.Message.Value);

                    var consumerResult = await this.handler.HandleAsync(this.consumer.Name, result.Message.Key, result.Message.Value);

                    if (consumerResult.Status == TaskStatus.RanToCompletion && consumerResult.Exception == null)
                    {
                        this.consumer.Commit(result);
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                Log.Logger.Warning("### Consumer operation cancelled {0}", oce.Message);
                Console.WriteLine($"Consumer operation cancelled  {oce.Message}");

                break;
            }
            catch (ConsumeException e)
            {
                // Consumer errors should generally be ignored (or logged) unless fatal.
                Console.WriteLine($"Consume error: {e.Error.Reason}");
                Log.Logger.Error("### Consume error {0}", e.Error.Reason);

                if (e.Error.IsFatal)
                {
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e}");
                Log.Logger.Error("### Consume setup error {0}", e);

                break;
            }
        }

        Log.Logger.Information("### Consumer started");

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

            Log.Logger.Information("### CONSUMER GETTING NEW TOKEN {0}", tokenEndpoint);

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
            var subject = GetTokenSubject(accessToken.AccessToken);
            var ms = tokenDate.ToUnixTimeMilliseconds();
            Log.Logger.Information("### CONSUMER GOT NEW TOKEN  {0} {1}", ms, tokenDate);

            client.OAuthBearerSetToken(accessToken.AccessToken, ms, subject);
        }
        catch (Exception ex)
        {
            Log.Logger.Error("### Token error {0}", ex.ToString());

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

    private static string GetTokenSubject(string token)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        return jwtSecurityToken.Claims.First(claim => claim.Type.Equals("sub", StringComparison.Ordinal)).Value;

    }
}

