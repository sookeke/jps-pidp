namespace Pidp.Kafka.Consumer.Handler;

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pidp.Data;
using Pidp.Kafka.Consumer.Model;
using Pidp.Kafka.Interfaces;
using Serilog;

public class NotificationAckHandler : IKafkaHandler<string, NotificationAckModel>
{
    private readonly PidpDbContext context;
    public NotificationAckHandler(PidpDbContext context) => this.context = context;

    public async Task<Task> HandleAsync(string consumerName, string key, NotificationAckModel value)
    {
        Log.Logger.Information("Received {0} message {1}", key, value.ToString());
        //check wheather this message has been processed before   
        if (await this.context.HasBeenProcessed(key, consumerName))
        {
            return Task.CompletedTask;
        }

        var accessRequest = await this.context.AccessRequests
            .Where(request => request.Id == value.AccessRequestId).SingleOrDefaultAsync();
        if (accessRequest != null)
        {
            using var trx = this.context.Database.BeginTransaction();

            try
            {
                accessRequest.Status = value.Status;
                await this.context.IdempotentConsumer(messageId: key, consumer: consumerName);
                await this.context.SaveChangesAsync();
                trx.Commit();
                Log.Logger.Information("Message {0} successfully processed");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Message {0} failed processing {1}", ex.Message);


                await trx.RollbackAsync();
                return Task.FromException(new InvalidOperationException());
            }
        }
        return Task.FromException(new InvalidOperationException()); //create specific exception handler later
    }
}
