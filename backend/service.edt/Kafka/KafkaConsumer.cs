namespace edt.service.Kafka;

using Confluent.Kafka;
using edt.service.Kafka.Interfaces;
using edt.service.ServiceEvents.UserAccountCreation.ConsumerRetry;
using edt.service.ServiceEvents.UserAccountCreation.Handler;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using static edt.service.EdtServiceConfiguration;

public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue> where TValue : class
{
    private readonly ConsumerConfig config;
    private IKafkaHandler<TKey, TValue> handler;
    private IConsumer<TKey, TValue> consumer;
    private IConsumer<TKey, TValue> retryConsumer;
    private string topic;
    private readonly RetryPolicy retryPolicy;
    private readonly EdtServiceConfiguration configuration;
    private IEnumerable<string> retryTopics;
    private const string EXPIRY_CLAIM = "exp";
    private const string SUBJECT_CLAIM = "sub";


    private readonly IServiceScopeFactory serviceScopeFactory;

    public KafkaConsumer(ConsumerConfig config, IServiceScopeFactory serviceScopeFactory, RetryPolicy retryPolicy, EdtServiceConfiguration configuration)
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.config = config;
        this.retryPolicy = retryPolicy;
        this.configuration = configuration;
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
                    var consumerResult = await this.handler.HandleAsync(this.consumer.MemberId, result.Message.Key, result.Message.Value);

                    if (consumerResult.Status == TaskStatus.RanToCompletion && consumerResult.Exception == null)
                    {
                        this.consumer.Commit(result);
                        this.consumer.StoreOffset(result);
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

            var tokenEndpoint = Environment.GetEnvironmentVariable("KafkaCluster__SaslOauthbearerTokenEndpointUrl");
            var clientId = Environment.GetEnvironmentVariable("KafkaCluster__SaslOauthbearerConsumerClientId");
            var clientSecret = Environment.GetEnvironmentVariable("KafkaCluster__SaslOauthbearerConsumerClientSecret");

            clientSecret ??= clusterConfig.GetValue<string>("KafkaCluster:SaslOauthbearerConsumerClientSecret");
            clientId ??= clusterConfig.GetValue<string>("KafkaCluster:SaslOauthbearerConsumerClientId");
            tokenEndpoint ??= clusterConfig.GetValue<string>("KafkaCluster:SaslOauthbearerTokenEndpointUrl");
            Log.Logger.Information("EDT Kafka Consumer getting token {0} {1} {2}", tokenEndpoint, clientId, clientSecret);

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
            Log.Logger.Information("Consumer got token {0}", ms);

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

    public async Task RetryConsume(List<RetryTopicModel> retryTopics, CancellationToken stoppingToken)
    {
        this.config.GroupId = this.configuration.KafkaCluster.RetryConsumerGroupId;
        using var scope = this.serviceScopeFactory.CreateScope();
        this.retryConsumer = new ConsumerBuilder<TKey, TValue>(this.config).SetOAuthBearerTokenRefreshHandler(OauthTokenRefreshCallback).SetValueDeserializer(new KafkaDeserializer<TValue>()).Build();
        this.retryTopics = retryTopics.Select(topic => topic.TopicName).ToList();

        await Task.Run(() => this.StartRetryConsumerLoop(stoppingToken), stoppingToken);

    }

    private async Task StartRetryConsumerLoop(CancellationToken cancellationToken)
    {
        this.retryConsumer.Subscribe(this.retryTopics);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = this.retryConsumer.Consume(cancellationToken);
                await this.RetryConsumerResult(result);
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

    private async Task RetryConsumerResult(ConsumeResult<TKey, TValue> result)
    {
        if (result != null)
        {

            Log.Information("Attempting retry {1} {0}", result.Topic, result.Message.Value);
            var retryMessage = result.Message;

            if (retryMessage != null)
            {
                var retryContext = new Polly.Context { { "retrycount", 0 } };
                var task = this.retryPolicy.RetryTasks[result.Topic];

                var consumerResult = await task.ExecuteAsync(async context =>
                    await this.handler.HandleRetryAsync(this.retryConsumer.MemberId,
                    result.Message.Key,
                    result.Message.Value,
                    (int)context["retrycount"],
                    result.Topic), retryContext);

                if (consumerResult.Status == TaskStatus.RanToCompletion && consumerResult.Exception == null)
                {
                    Log.Logger.Information("Retry processed successfully");
                    this.retryConsumer.Commit(result);
                    this.retryConsumer.StoreOffset(result);
                }
                else
                {
                    Log.Logger.Warning("Message was not processed successfully");
                }
                //    var consumerResult = await this.retryPolicy.ImmediateConsumerRetry.ExecuteAsync(
                //        async context => await this.handler.HandleRetryAsync(this.retryConsumer.MemberId, result.Message.Key, result.Message.Value, (int)context["retrycount"], result.Topic), retryContext);
                //
            }

            //if (result.Topic == this.configuration.KafkaCluster.InitialRetryTopicName)
            //{
            //    var retryContext = new Polly.Context { { "retrycount", 0 } };
            //    var consumerResult = await this.retryPolicy.ImmediateConsumerRetry.ExecuteAsync(
            //        async context => await this.handler.HandleRetryAsync(this.retryConsumer.MemberId, result.Message.Key, result.Message.Value, (int)context["retrycount"], result.Topic), retryContext);

            //    if (consumerResult.Status == TaskStatus.RanToCompletion && consumerResult.Exception == null)
            //    {
            //        this.retryConsumer.Commit(result);
            //        this.retryConsumer.StoreOffset(result);
            //    }
            //}
            //else if (result.Topic == this.configuration.KafkaCluster.MidRetryTopicName)
            //{
            //    var retryContext = new Polly.Context { { "retrycount", 0 } };
            //    var consumerResult = await this.retryPolicy.WaitForConsumerRetry.ExecuteAsync(
            //        async context => await this.handler.HandleRetryAsync(this.retryConsumer.MemberId, result.Message.Key, result.Message.Value, (int)context["retrycount"], result.Topic), retryContext);

            //    if (consumerResult.Status == TaskStatus.RanToCompletion && consumerResult.Exception == null)
            //    {
            //        this.retryConsumer.Commit(result);
            //        this.retryConsumer.StoreOffset(result);
            //    }
            //}
            //else if (result.Topic == this.configuration.KafkaCluster.FinalRetryTopicName)
            //{
            //    var retryContext = new Polly.Context { { "retrycount", 0 } };
            //    var consumerResult = await this.retryPolicy.FinalWaitForConsumerRetry.ExecuteAsync(
            //        async context => await this.handler.HandleRetryAsync(this.retryConsumer.MemberId, result.Message.Key, result.Message.Value, (int)context["retrycount"], result.Topic), retryContext);

            //    if (consumerResult.Status == TaskStatus.RanToCompletion && consumerResult.Exception == null)
            //    {
            //        this.retryConsumer.Commit(result);
            //        this.retryConsumer.StoreOffset(result);
            //    }
            //}
        }
    }
    public void CloseRetry() => this.retryConsumer.Close();
    public void DisposeRetry() => this.retryConsumer.Dispose();
}

