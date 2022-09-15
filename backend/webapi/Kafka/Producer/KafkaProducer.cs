namespace Pidp.Kafka.Producer;

using Confluent.Kafka;
using Pidp.Kafka.Interfaces;

public class KafkaProducer<TKey, TValue> : IDisposable, IKafkaProducer<TKey, TValue> where TValue : class
{
    private readonly IProducer<TKey, TValue> producer;
    public KafkaProducer(ProducerConfig config) => this.producer = new ProducerBuilder<TKey, TValue>(config).SetValueSerializer(new KafkaSerializer<TValue>()).Build();
    public async Task ProduceAsync(string topic, TKey key, TValue value) => await this.producer.ProduceAsync(topic, new Message<TKey, TValue> { Key = key, Value = value });
    public void Dispose()
    {
        this.producer.Flush();
        this.producer.Dispose();
    }
}
