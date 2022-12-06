namespace edt.service.ServiceEvents.UserAccountCreation.Handler;

using System.Globalization;
using edt.service.Data;
using edt.service.HttpClients.Services.EdtCore;
using edt.service.Kafka.Interfaces;
using edt.service.Kafka.Model;
using edt.service.ServiceEvents.UserAccountCreation.Models;
using Serilog;

public class UserProvisioningHandler : IKafkaHandler<string, EdtUserProvisioningModel>
{
    private readonly IKafkaProducer<string, Notification> producer;
    private readonly IKafkaProducer<string, UserModificationEvent> userModificationProducer;

    private readonly EdtServiceConfiguration configuration;
    private readonly IEdtClient edtClient;
    private readonly EdtDataStoreDbContext context;

    public UserProvisioningHandler(
        IKafkaProducer<string, Notification> producer,
        IKafkaProducer<string, UserModificationEvent> userModificationProducer,
        EdtServiceConfiguration configuration,
        IEdtClient edtClient,
        EdtDataStoreDbContext context)
    {
        this.producer = producer;
        this.userModificationProducer = userModificationProducer;
        this.configuration = configuration;
        this.context = context;
        this.edtClient = edtClient;
    }
    public async Task<Task> HandleAsync(string consumerName, string key, EdtUserProvisioningModel accessRequestModel)
    {
        //check wheather this message has been processed before   
        if (await this.context.HasBeenProcessed(key, consumerName))
        {
            return Task.CompletedTask;
        }
        // check edt service api is available before making any http request
        try
        {
            var version = await this.edtClient.GetVersion();
            Log.Logger.Debug("EDT {0}", version);
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }

        // check whether edt user already exists
        var user = await this.edtClient.GetUser(accessRequestModel.Key!);

        // Creating and updating users will result in a modification event that we'll
        // place onto a topic for others to consume
        var result = user == null
            ? await this.edtClient.CreateUser(accessRequestModel) //create user
            : await this.edtClient.UpdateUser(accessRequestModel, user); //update user

        Log.Logger.Information("User modification event {0}", result.ToString());

        if (result.Successful)
        {
            using var trx = this.context.Database.BeginTransaction();
            try
            {
                //add to tell message has been proccessed by consumer
                await this.context.IdempotentConsumer(messageId: key, consumer: consumerName);

                await this.context.SaveChangesAsync();

                // TODO - fix typo (is partyId used for anything?)
                await this.producer.ProduceAsync(this.configuration.KafkaCluster.ProducerTopicName, key: key, new Notification
                {
                    To = accessRequestModel.Email,
                    From = "jpsprovideridentityportal@gov.bc.ca",
                    FirstName = accessRequestModel.FullName!.Split(' ').FirstOrDefault(),
                    Subject = "Digital Evidence Management System Enrollment Confirmation",
                    MsgBody = MsgBody(accessRequestModel.FullName?.Split(' ').FirstOrDefault()),
                    ParyId = accessRequestModel.Key!,
                    Tag = Guid.NewGuid().ToString()
                });


                await trx.CommitAsync();

                // publish to the user creation topic for others to consume
                await this.userModificationProducer.ProduceAsync(this.configuration.KafkaCluster.UserCreationTopicName, key: key, result);

                return Task.CompletedTask;
            }
            catch (Exception)
            {
                await trx.RollbackAsync();
                return Task.FromException(new InvalidOperationException());
            }

        }
        else
        {
            // we should put a failure on a different topic
        } 

        return Task.FromException(new InvalidOperationException()); //create specific exception handler later
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
                <img src='https://drive.google.com/uc?export=view&id=16JU6XoVz5FvFUXXWCN10JvN-9EEeuEmr' width='' height='50'/><br/><br/><div style='border-top: 3px solid #22BCE5'><span style = 'font-family: Arial; font-size: 10pt' >
                <br/> Hello {0},<br/><br/> Your Digital Evidence Management System access request has been processed and your EDT account has been setup.<br/><br/>
                You can now Login to the <b>EDT Portal</b> with your digital identity via SSO <b></b> to access the Digital Evidence Management System by clicking on the above link. <br/><br/> Thanks <br/> DEMS User Management.<br/>
                In case of questions contact TBD<br/>
                </span></div></body></html> ",
                firstName);
        return msgBody;
    }

}


