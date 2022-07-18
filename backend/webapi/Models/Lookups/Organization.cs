namespace Pidp.Models.Lookups;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum OrganizationCode
{
    JusticeSector = 1,
    LawEnforcement,
    LawSociety,
    CorrectionService,
    HealthAuthority,
    BcGovernmentMinistry,
    ICBC,
    Other
}

[Table("OrganizationLookup")]
public class Organization
{
    [Key]
    public OrganizationCode Code { get; set; }

    public string Name { get; set; } = string.Empty;
}

public class OrganizationDataGenerator : ILookupDataGenerator<Organization>
{
    public IEnumerable<Organization> Generate() => new[]
    {
        new Organization { Code = OrganizationCode.JusticeSector,        Name = "Justice Sector" },
        new Organization { Code = OrganizationCode.LawEnforcement,       Name = "BC Law Enforcement"},
        new Organization { Code = OrganizationCode.LawSociety,           Name = "BC Law Society"},
        new Organization { Code = OrganizationCode.CorrectionService,    Name = "BC Corrections Service"},
        new Organization { Code = OrganizationCode.HealthAuthority,      Name = "Health Authority"       },
        new Organization { Code = OrganizationCode.BcGovernmentMinistry, Name = "BC Government Ministry" },
        new Organization { Code = OrganizationCode.ICBC,                 Name = "ICBC"                   },
        new Organization { Code = OrganizationCode.Other,                Name = "Other"                  }
    };
}
