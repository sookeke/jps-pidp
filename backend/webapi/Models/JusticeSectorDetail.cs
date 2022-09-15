namespace Pidp.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pidp.Models.Lookups;

[Table(nameof(JusticeSectorDetail))]
public class JusticeSectorDetail
{
    [Key]
    public int Id { get; set; }
    public JusticeSectorCode? JusticeSectorCode { get; set; }
    public JusticeSector? JusticeSector { get; set; }
    public string JustinUserId { get; set; } = string.Empty;
    public string ParticipantId { get; set; } = string.Empty;
    public PartyOrgainizationDetail OrgainizationDetail { get; set; } = new PartyOrgainizationDetail();

}
