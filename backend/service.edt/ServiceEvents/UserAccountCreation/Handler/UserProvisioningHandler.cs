namespace edt.service.ServiceEvents.UserAccountCreation.Handler;

using System.Globalization;
using edt.service.Data;
using edt.service.HttpClients.Services.EdtCore;
using edt.service.Kafka.Interfaces;
using edt.service.ServiceEvents.UserAccountCreation.Models;

public class UserProvisioningHandler : IKafkaHandler<string, EdtUserProvisioningModel>
{
    private readonly IKafkaProducer<string, Notification> producer;
    private readonly IKafkaProducer<string, EdtUserProvisioningModel> retryProducer;
    private readonly EdtServiceConfiguration configuration;
    private readonly IEdtClient edtClient;
    private readonly ILogger logger;
    private readonly EdtDataStoreDbContext context;

    public UserProvisioningHandler(
        IKafkaProducer<string, Notification> producer,
        EdtServiceConfiguration configuration,
        IEdtClient edtClient,
        EdtDataStoreDbContext context,
        IKafkaProducer<string, EdtUserProvisioningModel> retryProducer, ILogger logger)
    {
        this.producer = producer;
        this.configuration = configuration;
        this.context = context;
        this.logger = logger;
        this.edtClient = edtClient;
        this.retryProducer = retryProducer;
    }
    public async Task<Task> HandleAsync(string consumerName, string key, EdtUserProvisioningModel value)
    {
        using var trx = this.context.Database.BeginTransaction();
        try
        {
            //check wheather this message has been processed before   
            if (await this.context.HasBeenProcessed(key, consumerName))
            {
                return Task.CompletedTask;
            }
            ///check weather edt service api is available before making any http request
            ///
            /// call version endpoint via get
            ///

            //check wheather edt user already exist
            var result = await this.CheckUser(value);

            if (result)
            {
                //add to tell message has been proccessed by consumer
                await this.context.IdempotentConsumer(messageId: key, consumer: consumerName);

                await this.context.SaveChangesAsync();
                //After successful operation, we can produce message for other service's consumption e.g. Notification service

                // TODO - fix typo (is partyId used for anything?)
                await this.producer.ProduceAsync(this.configuration.KafkaCluster.ProducerTopicName, key: key, new Notification
                {
                    To = value.Email,
                    From = "jpsprovideridentityportal@gov.bc.ca",
                    FirstName = value.FullName!.Split(' ').FirstOrDefault(),
                    Subject = "Digital Evidence Management System Enrolment Confirmation",
                    MsgBody = MsgBody(value.FullName?.Split(' ').FirstOrDefault()),
                    ParyId = value.Key!,
                    Tag = Guid.NewGuid().ToString()
                });


                await trx.CommitAsync();

                return Task.CompletedTask;

            }
        }
        catch (Exception)
        {
            //on exception rollback trx, publish to retry topic for retry and commit offset 
            await trx.RollbackAsync();
            await this.PublishToErrorTopic(value, key, this.configuration.KafkaCluster.InitialRetryTopicName, this.configuration.RetryPolicy.InitialRetryTopicName.RetryCount, this.configuration.RetryPolicy.InitialRetryTopicName.WaitAfterInMins);
            this.logger.LogUserAccessPublishError(value.Key, key, this.configuration.KafkaCluster.ProducerTopicName, this.configuration.KafkaCluster.InitialRetryTopicName);
        }

        return Task.CompletedTask; //create specific exception handler later
    }

    private async Task<bool> CheckUser(EdtUserProvisioningModel value)
    {
        var user = await this.edtClient.GetUser(value.Key!);
        //create user account in EDT

        var result = user == null
            ? await this.edtClient.CreateUser(value) //&& await this.edtClient.AddUserGroup($"Key:{value.Key!}", value.AssignedRegion) //create user
            : await this.edtClient.UpdateUser(value, user);//update user
        return result;
    }

    /// <summary>
    ///
    /// TODO This should come from a template
    /// </summary>
    /// <param name="firstName"></param>
    /// <returns></returns>
    private static string MsgBody(string? firstName)
    {

        var msgBody = string.Format(CultureInfo.CurrentCulture, @"<html>
            <head>
                <title>Digital Evidence Management System Enrolment Confirmation</title>
            </head>
                <body> 
                <img src='https://drive.google.com/uc?export=view&id=16JU6XoVz5FvFUXXWCN10JvN-9EEeuEmr'width='' height='50'/><br/><br/><div style='border-top: 3px solid #22BCE5'><span style = 'font-family: Arial; font-size: 10pt' ><br/> Hello {0},<br/><br/> Your Digital Evidence Management System Access Request has been processed and account successfully provisioned.<br/><br/>
                You can Log in to the <a href='{1}'> EDT Portal </a> with your digital identity via SSO <b></b> to access the Digital Evidence Management System by clicking on the above link. <br/><br/> Thanks <br/> DEMS User Management.
                </span></div></body></html> ",
                firstName, "https://edtdems-poc.maple-edt.io/");
        return msgBody;
    }

    /// <summary>
    /// Publish failed event to retry topic
    /// </summary>
    /// <param name="consumerName"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="retryCount"></param>
    /// <param name="topicName"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task PublishToErrorTopic(EdtUserProvisioningModel value, string key, string topicName, int retryNumber, int timeInMin)
    {
        value.RetryNumber = retryNumber;
        value.RetryDuration = TimeSpan.FromMinutes(timeInMin);
        await this.retryProducer.ProduceAsync(topicName, key, value);
    }
    public async Task<Task> HandleRetryAsync(string consumerName, string key, EdtUserProvisioningModel value, int retryCount, string topicName)
    {
        using var trx = this.context.Database.BeginTransaction();
        try
        {
            //check wheather this message has been processed before   
            if (await this.context.HasBeenProcessed(key, consumerName))
            {
                return Task.CompletedTask;
            }
            ///check weather edt service api is available before making any http request
            ///
            /// call version endpoint via get
            ///

            //check wheather edt user already exist
            var result = await this.CheckUser(value);

            if (result)
            {
                //add to tell message has been proccessed by consumer
                await this.context.IdempotentConsumer(messageId: key, consumer: consumerName);

                await this.context.SaveChangesAsync();
                //After successful operation, we can produce message for other service's consumption e.g. Notification service

                // TODO - fix typo (is partyId used for anything?)
                await this.producer.ProduceAsync(this.configuration.KafkaCluster.ProducerTopicName, key: key, new Notification
                {
                    To = value.Email,
                    From = "jpsprovideridentityportal@gov.bc.ca",
                    FirstName = value.FullName!.Split(' ').FirstOrDefault(),
                    Subject = "Digital Evidence Management System Enrolment Confirmation",
                    MsgBody = MsgBody(value.FullName?.Split(' ').FirstOrDefault()),
                    ParyId = value.Key!,
                    Tag = Guid.NewGuid().ToString()
                });


                await trx.CommitAsync();

                return Task.CompletedTask;

            }
        }
        catch (Exception e)
        {
            if (retryCount == value.RetryNumber && topicName == this.configuration.KafkaCluster.InitialRetryTopicName)
            {
                //commit the offset at last trial and allow next consumer to retry
                await this.PublishToErrorTopic(value, key, this.configuration.KafkaCluster.MidRetryTopicName, this.configuration.RetryPolicy.MidRetryTopicName.RetryCount, this.configuration.RetryPolicy.MidRetryTopicName.WaitAfterInMins);
                this.logger.LogUserAccessPublishError(value.Key, key, this.configuration.KafkaCluster.InitialRetryTopicName, this.configuration.KafkaCluster.MidRetryTopicName);
            }
            else if (retryCount == value.RetryNumber && topicName == this.configuration.KafkaCluster.MidRetryTopicName)
            {
                //commit the offset at last trial and allow next consumer to retry
                await this.PublishToErrorTopic(value, key, this.configuration.KafkaCluster.MidRetryTopicName, this.configuration.RetryPolicy.MidRetryTopicName.RetryCount, this.configuration.RetryPolicy.MidRetryTopicName.WaitAfterInMins);
                this.logger.LogUserAccessPublishError(value.Key, key, this.configuration.KafkaCluster.MidRetryTopicName, this.configuration.KafkaCluster.FinalRetryTopicName);
            }
            else if (retryCount == value.RetryNumber && topicName == this.configuration.KafkaCluster.FinalRetryTopicName)
            {
                await this.context.FailedEventLogs.AddAsync(new FailedEventLog
                {
                    EventId = key,
                    Producer = this.configuration.KafkaCluster.ProducerTopicName,
                    ConsumerGroupId = this.configuration.KafkaCluster.RetryConsumerGroupId,
                    ConsumerId = consumerName,
                    EventPayload = value
                });
                await this.context.SaveChangesAsync();
                this.logger.LogUserAccessRetryError(value.Key!, key);
                return Task.FromException(e);
            }
            else
            {
                return Task.FromException(e);
            }

        }
        return Task.CompletedTask;
    }
}
public static partial class UserProvisioningHandlerLoggingExtensions
{
    [LoggerMessage(5, LogLevel.Warning, "Cannot provisioned user with partId {partId} and request Id {accessrequestId}. Published event key {accessrequestId} of {fromTopic} record to {topic} topic for retrial")]
    public static partial void LogUserAccessPublishError(this ILogger logger, string? partId, string accessrequestId, string fromTopic, string topic);
    [LoggerMessage(6, LogLevel.Error, "Error creating or updating edt user with partId {partId} and access requestId {accessRequestId} after final retry")]
    public static partial void LogUserAccessRetryError(this ILogger logger, string partId, string accessRequestId);

}


