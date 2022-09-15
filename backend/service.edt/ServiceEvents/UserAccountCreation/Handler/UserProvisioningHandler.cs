namespace edt.service.ServiceEvents.UserAccountCreation.Handler;

using edt.service.Data;
using edt.service.HttpClients.Services.EdtCore;
using edt.service.Kafka.Interfaces;

public class UserProvisioningHandler : IKafkaHandler<string, EdtUserProvisioningModel>
{
    private readonly IKafkaProducer<string, EdtUserProvisioningModel> producer;
    private readonly EdtServiceConfiguration configuration;
    private readonly IEdtClient edtClient;
    private readonly EdtDataStoreDbContext context;

    public UserProvisioningHandler(
        IKafkaProducer<string, EdtUserProvisioningModel> producer,
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
        //create user account in EDT

        var result = await this.edtClient.CreateUser(value);

        if (result)
        {
            using var trx = this.context.Database.BeginTransaction();
            try
            {
                //add to tell message has been proccessed by consumer
                await this.context.IdempotentConsumer(messageId: key, consumer: consumerName);

                await this.context.SaveChangesAsync();
                //After successful operation, we can produce message for other service's consumption

                await this.producer.ProduceAsync(this.configuration.KafkaCluster.ProducerTopicName, key: key, value);


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

}


