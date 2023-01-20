namespace edt.service.ServiceEvents.UserAccountCreation.Handler;

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using edt.service.Data;
using edt.service.Exceptions;
using edt.service.HttpClients;
using edt.service.HttpClients.Services.EdtCore;
using edt.service.Infrastructure.Telemetry;
using edt.service.Kafka;
using edt.service.Kafka.Interfaces;
using edt.service.Kafka.Model;
using edt.service.ServiceEvents.UserAccountCreation.Models;
using Microsoft.EntityFrameworkCore;
using static edt.service.EdtServiceConfiguration;

public class UserProvisioningHandler : IKafkaHandler<string, EdtUserProvisioningModel>
{
    private readonly IKafkaProducer<string, Notification> producer;
    private readonly IKafkaProducer<string, EdtUserProvisioningModel> retryProducer;
    private readonly IKafkaProducer<string, UserModificationEvent> userModificationProducer;

    private readonly EdtServiceConfiguration configuration;
    private readonly IEdtClient edtClient;
    private readonly ILogger logger;
    private readonly EdtDataStoreDbContext context;



    public UserProvisioningHandler(
        IKafkaProducer<string, Notification> producer,
        IKafkaProducer<string, UserModificationEvent> userModificationProducer,
        EdtServiceConfiguration configuration,
        IEdtClient edtClient,
        EdtDataStoreDbContext context,
        IKafkaProducer<string, EdtUserProvisioningModel> retryProducer, ILogger logger)
    {
        this.producer = producer;
        this.userModificationProducer = userModificationProducer;
        this.configuration = configuration;
        this.context = context;
        this.logger = logger;
        this.edtClient = edtClient;
        this.retryProducer = retryProducer;
    }

    public async Task<Task> HandleAsync(string consumerName, string key, EdtUserProvisioningModel accessRequestModel)
    {

        Serilog.Log.Logger.Information("Db {0} {1}", this.context.Database.CanConnect(), this.context.Database.GetConnectionString());

        // set acitivty info
        Activity.Current?.AddTag("digitalevidence.party.id", accessRequestModel.AccessRequestId);


        using var trx = this.context.Database.BeginTransaction();
        try
        {
            //check wheather this message has been processed before   
            if (await this.context.HasBeenProcessed(key, consumerName))
            {
                //await trx.RollbackAsync();
                return Task.CompletedTask;
            }
            ///check weather edt service api is available before making any http request
            ///
            /// call version endpoint via get


            //check wheather edt user already exist
            var result = await this.CheckUser(accessRequestModel);


            if (result.successful)
            {
                //add to tell message has been proccessed by consumer
                await this.context.IdempotentConsumer(messageId: key, consumer: consumerName);

                await this.context.SaveChangesAsync();

                var msgKey = Guid.NewGuid().ToString();


                // TODO - fix typo (is partyId used for anything?)
                await this.producer.ProduceAsync(this.configuration.KafkaCluster.ProducerTopicName, key: key, new Notification
                {
                    To = accessRequestModel.Email,
                    From = "jpsprovideridentityportal@gov.bc.ca",
                    FirstName = accessRequestModel.FullName!.Split(' ').FirstOrDefault(),
                    Subject = "Digital Evidence Management System Enrollment Confirmation",
                    MsgBody = MsgBody(accessRequestModel.FullName?.Split(' ').FirstOrDefault()),
                    PartyId = accessRequestModel.Key!,
                    Tag = msgKey
                });

                if ( string.IsNullOrEmpty(this.configuration.SchemaRegistry.Url))
                {
                    throw new EdtServiceException("Schema registry is not configured");
                }

                var producer = new SchemaAwareProducer(ConsumerSetup.GetProducerConfig(), this.userModificationProducer, this.configuration);
                // publish to the user creation topic for others to consume
                bool publishResultOk;
                if (result.eventType == UserModificationEvent.UserEvent.Create)
                {
                    Serilog.Log.Information("Publishing EDT user creation event {0} {1}", msgKey, accessRequestModel.Key);
                    publishResultOk = await producer.ProduceAsync(this.configuration.KafkaCluster.UserCreationTopicName, key: msgKey, result);
                }
                else
                {
                    Serilog.Log.Information("Publishing EDT user modification event {0} {1}", msgKey, accessRequestModel.Key);
                    publishResultOk = await producer.ProduceAsync(this.configuration.KafkaCluster.UserModificationTopicName, key: msgKey, result);
                }


                if (publishResultOk)
                {
                    await trx.CommitAsync();
                }
                else
                {
                    Serilog.Log.Logger.Error("Failed to publish to user notification topic - rolling back transaction");
                    await trx.RollbackAsync();
                }

                return Task.FromResult(publishResultOk);

            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Logger.Error("Exception during EDT provisioning {0}", ex.Message);
            //on exception rollback trx, publish to retry topic for retry and commit offset 
            //await trx.RollbackAsync();
            // publish to the initial error topic
            var initialRetryTopic = this.configuration.RetryPolicy.RetryTopics.Find(retryTopic => retryTopic.Order == 1);
            if (initialRetryTopic == null)
            {
                throw new EdtServiceException("Unable to locate retry topic with Order=1");
            }
            else
            {
                Serilog.Log.Information("Adding retry entry to {0}", initialRetryTopic.TopicName);
                await this.PublishToRetryTopic(accessRequestModel, key, initialRetryTopic);
            }
            // this.logger.LogUserAccessPublishError(accessRequestModel.Key, key, this.configuration.KafkaCluster.ProducerTopicName, this.configuration.KafkaCluster.InitialRetryTopicName);
        }

        return Task.CompletedTask; //create specific exception handler later
    }


    private async Task<string> CheckEdtServiceVersion() => await this.edtClient.GetVersion();

    private async Task<UserModificationEvent> CheckUser(EdtUserProvisioningModel value)
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
                <title>Digital Evidence Management System Enrollment Confirmation</title>
            </head>
            <body> 
                <img src='https://drive.google.com/uc?export=view&id=16JU6XoVz5FvFUXXWCN10JvN-9EEeuEmr' width='' height='50'/><br/><br/>
                <div style='border-top: 3px solid #22BCE5'><span style = 'font-family: Arial; font-size: 10pt' >
                <br/> Hello {0},<br/>
                <br/>Your BCPS DEMS profile has been successfully created.<p/>
                <br/><i>Your account is still being finalized and assigned cases will be available once this process completes.</i>
                <br/>If you need immediate access to a case you do not have access to, please contact DEMS Support.
                <p/>{1}<p/>
                </span></div>
            </body></html> ",
                firstName, GetSupportMessage());
        return msgBody;
    }

    private static string MsgBodyFailed(string? firstName)
    {

        var msgBody = string.Format(CultureInfo.CurrentCulture, @"<html>
            <head>
                <title>Digital Evidence Management System Enrollment Confirmation</title>
            </head>
                <body> 
                <img src='https://drive.google.com/uc?export=view&id=16JU6XoVz5FvFUXXWCN10JvN-9EEeuEmr' width='' height='50'/><br/><br/>
                <div style='border-top: 3px solid red'>
                <span style = 'font-family: Arial; font-size: 10pt' >
                <br/> Hello {0},<br/>
                <br/>Your BCPS DEMS profile has NOT been created.<p/><p/>
                {1}<br/>
                </span><p/><div style='border-top: 3px solid red'></div></body></html> ",
                firstName, GetSupportMessage());
        return msgBody;
    }

    private static string MsgBodyRetry(string? firstName, RetryTopicModel retryTopicModel)
    {
        var retryText = $"We will retry again in {retryTopicModel.DelayMinutes} minutes [{retryTopicModel.TopicName}]";

        var msgBody = string.Format(CultureInfo.CurrentCulture, @"<html>
            <head>
                <title>Digital Evidence Management System Enrollment Notification</title>
            </head>
                <body> 
                <img src='https://drive.google.com/uc?export=view&id=16JU6XoVz5FvFUXXWCN10JvN-9EEeuEmr' width='' height='50'/><br/><br/>
    <div style='border-top: 3px solid #22BCE5'><span style = 'font-family: Arial; font-size: 10pt' >
<br/> Hello {0},<br/><br/>We are currently experiencing problems completing your on-boarding request.<br/>
  <br/>{1}<br/>
We will inform you if we are unable to complete your request.<p/><p/>{2}<p/>
<div style='border-top: 3px solid #22BCE5'>
                </span></div></body></html> ",
                firstName, retryText, GetSupportMessage());
        return msgBody;
    }

    private static string GetSupportMessage() => "<p/>If you require any assistance, please contact <a href = \"mailto:bcps.disclosure.support@gov.bc.ca\">bcps.disclosure.support@gov.bc.ca</a><p/><p/>Thank you,<br/>BCPS DEMS Support<p/>";


    /// <summary>
    /// Publish to the retry topic
    /// </summary>
    /// <param name="value"></param>
    /// <param name="key"></param>
    /// <param name="retryTopicModel"></param>
    /// <returns></returns>
    private async Task PublishToRetryTopic(EdtUserProvisioningModel value, string key, RetryTopicModel retryTopicModel)
    {
        Serilog.Log.Information("Publishing to retry topic {0} {1}", value.Key, retryTopicModel.TopicName);
        // different topic then reset to 1, otherwise increment retry count

        value.RetryNumber = retryTopicModel.Order == value.TopicOrder ? value.RetryNumber : 1;
        value.RetryDuration = TimeSpan.FromMinutes(retryTopicModel.DelayMinutes);
        value.TopicOrder = retryTopicModel.Order;
        if (retryTopicModel.RetryCount >= value.RetryNumber)
        {
            // place onto the topic 
            await this.retryProducer.ProduceAsync(retryTopicModel.TopicName, key, value);

            var msgId = Guid.NewGuid().ToString();

            // if notification set and first retry attempt then send a message
            // we only want to notify once per retry topic unless NotifyOnEachRetry is set
            if (retryTopicModel.NotifyUser && (value.RetryNumber == 1 || retryTopicModel.NotifyOnEachRetry))
            {
                Serilog.Log.Information("Sending email to user to notify of retry");
                await this.producer.ProduceAsync(this.configuration.KafkaCluster.ProducerTopicName, key: msgId, new Notification
                {
                    To = value.Email,
                    From = "jpsprovideridentityportal@gov.bc.ca",
                    FirstName = value.FullName!.Split(' ').FirstOrDefault(),
                    Subject = "Digital Evidence Management System Notification",
                    MsgBody = MsgBodyRetry(value.FullName?.Split(' ').FirstOrDefault(), retryTopicModel),
                    PartyId = value.Key!,
                    Tag = msgId
                });

            }
        }
    }

    private async Task PublishToDeadLetterTopic(EdtUserProvisioningModel value, string key, string deadLetterTopic)
    {
        Serilog.Log.Information("Publishing to dead letter topic {0} {1}", value.Key);
        await this.retryProducer.ProduceAsync(deadLetterTopic, key, value);
    }

    private async Task NotifyUserFailure(EdtUserProvisioningModel value, string key, string topic)
    {
        var msgId = Guid.NewGuid().ToString();

        await this.producer.ProduceAsync(topic, key: msgId, new Notification
        {
            To = value.Email,
            From = "jpsprovideridentityportal@gov.bc.ca",
            FirstName = value.FullName!.Split(' ').FirstOrDefault(),
            Subject = "Digital Evidence Management System Notification",
            MsgBody = MsgBodyFailed(value.FullName?.Split(' ').FirstOrDefault()),
            PartyId = value.Key!,
            Tag = msgId
        });
    }


        public async Task<Task> HandleRetryAsync(string consumerName, string key, EdtUserProvisioningModel value, int retryCount, string topicName)
    {
        using var trx = this.context.Database.BeginTransaction();

        try
        {

            //check wheather this message has been processed before   
            if (await this.context.HasBeenProcessed(key, consumerName))
            {
                await trx.RollbackAsync();
                return Task.CompletedTask;
            }
            ///check weather edt service api is available before making any http request
            ///
            /// call version endpoint via get
            ///
            var edtVersion = await this.CheckEdtServiceVersion();

            if (edtVersion == null)
            {
                await trx.RollbackAsync();
                Serilog.Log.Logger.Error("Failed to ping EDT service");
                return Task.FromException(new EdtServiceException("Unable to access EDT endpoint"));
            }

            //check wheather edt user already exist
            var result = await this.CheckUser(value);

            //// TODO REMOVE THIS BLOCK ONCE TESTED
            //Serilog.Log.Error("THROWING FAKE EXCEPTION!!!!!!!");
            //throw new EdtServiceException("FAKE EXCEPTION!!!");
            //// TODO REMOVE THIS BLOCK ONCE TESTED


            if (result.successful)
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
                    Subject = "Digital Evidence Management System Enrollment Confirmation",
                    MsgBody = MsgBody(value.FullName?.Split(' ').FirstOrDefault()),
                    PartyId = value.Key!,
                    Tag = Guid.NewGuid().ToString()
                });


                await trx.CommitAsync();

                return Task.CompletedTask;

            }

        }
        catch (Exception e)
        {

            await trx.RollbackAsync();
            // get the last retry number
            var currentTopic = this.configuration.RetryPolicy.RetryTopics.Find(retryTopic => retryTopic.Order == value.TopicOrder);
            if (currentTopic == null)
            {
                throw new EdtServiceException($"Did not find a topic with order number {value.TopicOrder}");
            }


            if (value.RetryNumber >= currentTopic.RetryCount)
            {

                // skip to the next retry topic - if no topics left then send to dead letter topic and inform user of failure
                var nextRetryModel = this.configuration.RetryPolicy.RetryTopics.Find(retryTopic => retryTopic.Order == value.TopicOrder + 1);

                if (nextRetryModel == null)
                {
                    Serilog.Log.Warning("No more retry topics found - sending to dead letter topic");
                    await this.context.FailedEventLogs.AddAsync(new FailedEventLog
                    {
                        EventId = Guid.NewGuid().ToString(),
                        Producer = this.configuration.KafkaCluster.ProducerTopicName,
                        ConsumerGroupId = this.configuration.KafkaCluster.RetryConsumerGroupId,
                        ConsumerId = consumerName,
                        EventPayload = value
                    });

                    // publish to dead letter topic
                    await this.PublishToDeadLetterTopic(value, key, this.configuration.RetryPolicy.DeadLetterTopic);


                    // notify user
                    await this.NotifyUserFailure(value, key, this.configuration.KafkaCluster.ProducerTopicName);

                    await this.context.SaveChangesAsync();
                    this.logger.LogUserAccessRetryError(value.Key!, key);

                    // we didnt complete the request but we want the offset committed so we
                    // dont continue to process the messages
                    return Task.CompletedTask;
                }
                else
                {
                    Serilog.Log.Information("Moving to next retry topic");
                    await this.PublishToRetryTopic(value, key, nextRetryModel);
                }
            }
            else
            {
                // increase the retryCount and publish to same topic (if the timeout has been reached)
                var retryMessage = value;
                retryMessage.RetryNumber++;
                Serilog.Log.Information("Resending message to retry topic");
                await this.PublishToRetryTopic(retryMessage, key, currentTopic);
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


