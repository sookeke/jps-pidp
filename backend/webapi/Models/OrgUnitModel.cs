namespace Pidp.Models;

public class OrgUnitModel
{
    public string AssignedAgency { get; set; } = string.Empty;
    public int RegionId { get; set; }
    public string RegionName { get; set; } = string.Empty;
}
