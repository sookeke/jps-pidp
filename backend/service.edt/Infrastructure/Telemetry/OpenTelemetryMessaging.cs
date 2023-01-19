namespace edt.service.Infrastructure.Telemetry;

public static class OpenTelemetryMessaging
{
    /// <summary>
    /// Message system. For Kafka, attribute value must be "kafka".
    /// </summary>
    public const string SYSTEM = "messaging.system";

    /// <summary>
    /// Message destination. For Kafka, attribute value must be a Kafka topic.
    /// </summary>
    public const string DESTINATION = "messaging.destination";

    /// <summary>
    /// Destination kind. For Kafka, attribute value must be "topic".
    /// </summary>
    public const string DESTINATION_KIND = "messaging.destination_kind";

    /// <summary>
    /// Kafka partition number.
    /// </summary>
    public const string KAFKA_PARTITION = "messaging.kafka.partition";

    /// <summary>
    /// Kafka message key.
    /// </summary>
    public const string KAFKA_MESSAGE_KEY = "messaging.kafka.message_key";

    /// <summary>
    /// Kafka message payload size (bytes).
    /// </summary>
    public const string MESSAGE_PAYLOAD_SIZE_BYTES = "messaging.message_payload_size_bytes";
}
