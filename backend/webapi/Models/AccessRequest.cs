namespace Pidp.Models;

using NodaTime;
using Pidp.Models.Lookups;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


[Table(nameof(AccessRequest))]
public class AccessRequest : BaseAuditable
{
    [Key]
    public int Id { get; set; }

    public int PartyId { get; set; }

    public Party? Party { get; set; }

    public Instant RequestedOn { get; set; }

    public AccessTypeCode AccessTypeCode { get; set; }
    public string Status { get; set; } = AccessRequestStatus.Pending;
}
public static class AccessRequestStatus
{
    public const string Accepted = "Accepted";
    public const string Cancelled = "Cancelled";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Pending = "Pending";
}

[Table(nameof(HcimAccountTransfer))]
public class HcimAccountTransfer : AccessRequest
{
    public string LdapUsername { get; set; } = string.Empty;
}

[Table(nameof(HcimEnrolment))]
public class HcimEnrolment : AccessRequest
{
    public bool ManagesTasks { get; set; }
    public bool ModifiesPhns { get; set; }
    public bool RecordsNewborns { get; set; }
    public bool SearchesIdentifiers { get; set; }
}
[Table(nameof(DigitalEvidence))]
public class DigitalEvidence : AccessRequest
{
    public string OrganizationType { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string ParticipantId { get; set; } = string.Empty;
    [Column(TypeName = "jsonb")]
    public List<AssignedRegion> AssignedRegions { get; set; } = new List<AssignedRegion>();
}

public class AssignedRegion
{
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public string AssignedAgency { get; set; } = string.Empty;

}
