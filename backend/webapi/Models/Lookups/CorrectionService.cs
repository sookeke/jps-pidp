namespace Pidp.Models.Lookups;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum CorrectionServiceCode
{
    Incustody = 1,
    OutofCustody,
    Both
}
[Table("CorrectionServiceLookup")]
public class CorrectionService
{
    [Key]
    public CorrectionServiceCode Code { get; set; }
    public string Name { get; set; } = string.Empty;
}
public class CorrectionServiceDataGenerator : ILookupDataGenerator<CorrectionService>
{
    public IEnumerable<CorrectionService> Generate() => new[]
    {
        new CorrectionService{Code = CorrectionServiceCode.Incustody, Name = "In Custody"},
        new CorrectionService{Code = CorrectionServiceCode.OutofCustody, Name = "Out Of Custody"},
        new CorrectionService{Code = CorrectionServiceCode.Both, Name = "In and Out Of Custody"}
    };
}
