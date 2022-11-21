namespace edt.service.HttpClients.Services.EdtCore;

using System.Text.Json;

public class EdtUserProvisioningModel
{
    public string? Id { get; set; }
    public string? Key { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public List<AssignedRegion>? AssignedRegions { get; set; }
    public string? Role { get; set; }
    public bool? IsActive => true;
    public string? AccountType { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this);

}

public class AssignedRegion
{
    public int RegionId { get; set; }
    public string? RegionName { get; set; }
    public string? AssignedAgency { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this);

}
