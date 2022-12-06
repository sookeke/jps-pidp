namespace edt.service.Kafka.Model;
using NodaTime;

public abstract class AuditEvent
{
    public Instant EventTime { get; set; }

}
