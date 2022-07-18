namespace Pidp.Models.Lookups;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum JusticeSectorCode
{
    BCPS = 1,
    RSBC
}
[Table("JusticeSectorLookup")]
public class JusticeSector
{
    [Key]
    public JusticeSectorCode Code { get; set; }
    public string Name { get; set; } = string.Empty;
}
public class JusticeSectorDataGenerator : ILookupDataGenerator<JusticeSector>
{
    public IEnumerable<JusticeSector> Generate() => new []
    {
        new JusticeSector{Code = JusticeSectorCode.BCPS, Name = "BC Prosecution Service"},
        new JusticeSector{Code = JusticeSectorCode.RSBC, Name = "Road Safety BC"}
    };
}