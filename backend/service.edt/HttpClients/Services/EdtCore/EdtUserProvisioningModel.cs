namespace edt.service.HttpClients.Services.EdtCore;

public class EdtUserProvisioningModel
{
    public string? Id { get; set; }
    public string? Key { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public List<string> Group { get; set; } = new List<string>();
    public string? Role { get; set; }
    public bool? IsActive => true;
    public string? AccountType { get; set; }

}
