namespace edt.service.Kafka.Model;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;



/// <summary>
/// Represents a change to a user in relation to EDT
/// </summary>
public class UserModificationEvent : AuditEvent
{

    public enum UserEvent
    {
        Create,
        Modify,
        Delete,
        Disable,
        Enable
    }

    public string PartId { get; set; } = string.Empty;

    [JsonConverter(typeof(StringEnumConverter))]
    public UserEvent Event { get; set; } = UserEvent.Create;

    public int AccessRequestId { get; set; }

    [JsonIgnore]
    public bool Successful { get; set; }

    public override string ToString() => JsonConvert.SerializeObject(this);


}
