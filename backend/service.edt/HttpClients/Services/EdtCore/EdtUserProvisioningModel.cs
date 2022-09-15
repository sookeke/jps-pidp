namespace edt.service.HttpClients.Services.EdtCore;

public class EdtUserProvisioningModel
{
    public string? Key { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool? IsActive => true;
    public int? AccountType { get; set; }

}
