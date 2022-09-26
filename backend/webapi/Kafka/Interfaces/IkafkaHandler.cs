namespace Pidp.Kafka.Interfaces;

public interface IKafkaHandler<TK, TV>
{
    /// <summary>
    /// Provide mechanism to handle the consumer message from Kafka
    /// </summary>
    /// <param name="key">Indicates the message's key for Kafka Topic</param>
    /// <param name="value">Indicates the message's value for Kafka Topic</param>
    /// <returns></returns>
    Task<Task> HandleAsync(string consumerName, TK key, TV value);
}
