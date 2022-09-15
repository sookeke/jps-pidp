namespace edt.service.HttpClients.Services.EdtCore;

using NodaTime;

public class AccessRequest
{
    public Guid Id { get; set; }

    public int PartyId { get; set; }

    public Instant RequestedOn { get; set; }

    public AccessTypeCode AccessTypeCode { get; set; }
}
public class DigitalEvidence : AccessRequest
{
    public string UserType { get; set; } = string.Empty;
    public string ParticipantId { get; set; } = string.Empty;
}
public enum AccessTypeCode
{
    SAEforms = 1,
    HcimAccountTransfer,
    HcimEnrolment,
    DriverFitness,
    DigitalEvidence,
    Uci,
    MSTeams
}
