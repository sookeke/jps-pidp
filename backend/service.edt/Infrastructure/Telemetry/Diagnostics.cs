namespace edt.service.Infrastructure.Telemetry;

using Confluent.Kafka;
using System.Diagnostics;
using System.Text;


internal static class Diagnostics
{
    private const string ActivitySourceName = "Confluent.Kafka";
    public static ActivitySource ActivitySource { get; } = new ActivitySource(ActivitySourceName);


    internal static class Producer
    {
        private const string ActivityName = ActivitySourceName + ".MessageProduced";

        internal static Activity Start<TKey, TValue>(string topic, Message<TKey, TValue> message)
        {
            var activity = ActivitySource.StartActivity(ActivityName);

            if (activity == null)
                return null;

            using (activity)
            {
                activity?.AddDefaultOpenTelemetryTags(topic, message);
            }

            return activity;
        }
    }

    private static Activity AddDefaultOpenTelemetryTags<TKey, TValue>(
        this Activity activity,
        string topic,
        Message<TKey, TValue> message)
    {
        activity?.AddTag(OpenTelemetryMessaging.SYSTEM, "kafka");
        activity?.AddTag(OpenTelemetryMessaging.DESTINATION, topic);
        activity?.AddTag(OpenTelemetryMessaging.DESTINATION_KIND, "topic");

        if (message.Key != null)
            activity?.AddTag(OpenTelemetryMessaging.KAFKA_MESSAGE_KEY, message.Key);

        if (message.Value != null)
        {
            var messagePayloadBytes = Encoding.UTF8.GetByteCount(message.Value.ToString());
            activity?.AddTag(OpenTelemetryMessaging.MESSAGE_PAYLOAD_SIZE_BYTES, messagePayloadBytes.ToString());
        }

        return activity;
    }
}
