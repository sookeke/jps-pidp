namespace Pidp.Models.Lookups;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum LawSocietyCode
{
    BCLawSociety = 1
}
[Table("LawSocietyLookup")]
public class LawSociety
{
    [Key]
    public LawSocietyCode Code { get; set; }
    public string Name { get; set; } = string.Empty;
}
public class LawSocietyDataGenerator : ILookupDataGenerator<LawSociety>
{
    public IEnumerable<LawSociety> Generate() => new[]
    {
        new LawSociety{Code = LawSocietyCode.BCLawSociety, Name = "BC Law Society"}
    };
}
