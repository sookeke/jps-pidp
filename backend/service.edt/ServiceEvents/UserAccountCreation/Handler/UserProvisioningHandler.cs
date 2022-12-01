namespace edt.service.ServiceEvents.UserAccountCreation.Handler;

using System.Globalization;
using edt.service.Data;
using edt.service.HttpClients.Services.EdtCore;
using edt.service.Kafka.Interfaces;
using edt.service.ServiceEvents.UserAccountCreation.Models;

public class UserProvisioningHandler : IKafkaHandler<string, EdtUserProvisioningModel>
{
    private readonly IKafkaProducer<string, Notification> producer;
    private readonly EdtServiceConfiguration configuration;
    private readonly IEdtClient edtClient;
    private readonly EdtDataStoreDbContext context;

    public UserProvisioningHandler(
        IKafkaProducer<string, Notification> producer,
        EdtServiceConfiguration configuration,
        IEdtClient edtClient,
        EdtDataStoreDbContext context)
    {
        this.producer = producer;
        this.configuration = configuration;
        this.context = context;
        this.edtClient = edtClient;
    }
    public async Task<Task> HandleAsync(string consumerName, string key, EdtUserProvisioningModel value)
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
        var user = await this.edtClient.GetUser(value.Key!);
        //create user account in EDT

        var result = user == null
            ? await this.edtClient.CreateUser(value) //&& await this.edtClient.AddUserGroup($"Key:{value.Key!}", value.AssignedRegion) //create user
            : await this.edtClient.UpdateUser(value, user);//update user


        if (result)
        {
            using var trx = this.context.Database.BeginTransaction();
            try
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
                    ParyId = value.Key!,
                    Tag = Guid.NewGuid().ToString()
                });


                await trx.CommitAsync();

                return Task.CompletedTask;
            }
            catch (Exception)
            {
                await trx.RollbackAsync();
                return Task.FromException(new InvalidOperationException());
            }

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
                <img src='data:image/png;base64, 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' width='' height='50'/><br/><br/><div style='border-top: 3px solid #22BCE5'><span style = 'font-family: Arial; font-size: 10pt' >
                <br/> Hello {0},<br/><br/> Your Digital Evidence Management System access request has been processed and your EDT account has been setup.<br/><br/>
                You can now Login to the <b>EDT Portal</b> with your digital identity via SSO <b></b> to access the Digital Evidence Management System by clicking on the above link. <br/><br/> Thanks <br/> DEMS User Management.<br/>
                In case of questions contact TBD<br/>
                </span></div></body></html> ",
                firstName);
        return msgBody;
    }

}


