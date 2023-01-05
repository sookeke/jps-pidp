namespace edt.service.ServiceEvents.UserAccountCreation.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using edt.service.HttpClients.Services.EdtCore;
using Newtonsoft.Json;

public class FailedEventLog : BaseAuditable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string? EventId { get; set; }
    public string? Producer { get; set; }
    public string? ConsumerGroupId { get; set; }
    public string? ConsumerId { get; set; }
    [NotMapped]
    public EdtUserProvisioningModel? EventPayload
    {
        get => (this.JsonEventPayload == null) ? null : JsonConvert.DeserializeObject<EdtUserProvisioningModel>(this.JsonEventPayload);
        set => this.JsonEventPayload = JsonConvert.SerializeObject(value);
    }
    internal string JsonEventPayload { get; set; } = string.Empty;
}
