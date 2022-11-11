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
        var user = await this.edtClient.GetUser(value);
        //create user account in EDT

        //var result = user != null
        //    || await this.edtClient.CreateUser(value);

        var result = user == null
            ? await this.edtClient.CreateUser(value) && await this.edtClient.AddUserGroup($"Key:{value.Key!}", value.Group) //create user
            : await this.edtClient.UpdateUser(value);//update user

        if (result)
        {
            using var trx = this.context.Database.BeginTransaction();
            try
            {
                //add to tell message has been proccessed by consumer
                await this.context.IdempotentConsumer(messageId: key, consumer: consumerName);

                await this.context.SaveChangesAsync();
                //After successful operation, we can produce message for other service's consumption e.g. Notification service

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
            catch (Exception)
            {
                await trx.RollbackAsync();
                return Task.FromException(new InvalidOperationException());
            }

        }

        return Task.FromException(new InvalidOperationException()); //create specific exception handler later
    }
    private static string MsgBody(string? firstName)
    {
        var msgBody = string.Format(CultureInfo.CurrentCulture, @"<html>
            <head>
                <title>Digital Evidence Management System Enrolment Confirmation</title>
            </head>
                <body> 
                <img src='https://drive.google.com/uc?export=view&id=16JU6XoVz5FvFUXXWCN10JvN-9EEeuEmr'width='' height='50'/><br/><br/><div style='border-top: 3px solid #22BCE5'><span style = 'font-family: Arial; font-size: 10pt' ><br/> Hello {0},<br/><br/> Your Digital Evidence Management System Access Request has been processed and account succesfully provisioned.<br/><br/>
                You can Log in to the <a href='{1}'> EDT Portal </a> with your digital identity via SSO <b></b> to access the Digital Evidence Management System by clicking on the above link. <br/><br/> Thanks <br/> DEMS User Management.
                </span></div></body></html> ",
                firstName, "https://edtdems-poc.maple-edt.io/");
        return msgBody;
    }

}


