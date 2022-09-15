namespace Pidp.Models.OutBoxEvent;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

[Table(nameof(ExportedEvent))]
public class ExportedEvent
{
    [Required]
    public int EventId { get; set; }
    [Required]
    public string AggregateType { get; set; } = string.Empty;
    [Required]
    public string? AggregateId { get; set; }
    [Required]
    public string EventType { get; set; } = string.Empty;
    [Required]
    //public JsonNode? EventPayload { get; set; }
    [NotMapped]
    public EdtUserProvisioning? EventPayload
    {
        get => (this.JsonEventPayload == null) ? null : JsonConvert.DeserializeObject<EdtUserProvisioning>(this.JsonEventPayload);
        set => this.JsonEventPayload = JsonConvert.SerializeObject(value);
    }
    internal string JsonEventPayload { get; set; } = string.Empty;
}
