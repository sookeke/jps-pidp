namespace Pidp.Models.Lookups;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum LawEnforcementCode
{
    RCMP = 1,
    VicPD,
    SannichPD,
    DeltaPD
}

[Table("LawEnforcementLookup")]
public class LawEnforcement
{
    [Key]
    public LawEnforcementCode Code { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class LawEnforcementDataGenerator : ILookupDataGenerator<LawEnforcement>
{
    public IEnumerable<LawEnforcement> Generate() => new[]
    {
        new LawEnforcement { Code = LawEnforcementCode.RCMP, Name= "Royal Canadian Mounted Police"},
        new LawEnforcement { Code = LawEnforcementCode.VicPD, Name = "Victoria Police Department"},
        new LawEnforcement { Code = LawEnforcementCode.SannichPD, Name = "Sannich Police Department"},
        new LawEnforcement { Code = LawEnforcementCode.DeltaPD, Name = "Delta Police Departmemt"},
    };
}
