namespace Pidp.Models;

using Pidp.Models.Lookups;

public class CorrectionServiceDetail
{
    public int Id { get; set; }
    public CorrectionServiceCode? CorrectionServiceCode { get; set; }
    public CorrectionService? CorrectionService { get; set; }
    public string PeronalId { get; set; } = string.Empty;
    public PartyOrgainizationDetail OrgainizationDetail { get; set; } = new PartyOrgainizationDetail();

}
