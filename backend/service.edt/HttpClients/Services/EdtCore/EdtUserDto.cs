namespace edt.service.HttpClients.Services.EdtCore;
using System.Text.Json;

public class EdtUserDto
{
    public string? Id { get; set; }
    public string? Key { get; set; }
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool? IsActive => true;
    public string? AccountType { get; set; }

    // tostring that provides details of the object
    public override string ToString() => JsonSerializer.Serialize(this);

}
