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
    public string UserType { get; set; } = string.Empty;
}
