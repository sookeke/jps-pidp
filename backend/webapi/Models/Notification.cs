namespace Pidp.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents an email (or other method later) notification model
/// </summary>
public class Notification
{
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string? To { get; set; }
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string? From { get; set; }
    public string? Subject { get; set; }
    public string? FirstName { get; set; }
    public string? MsgBody { get; set; }
    public string PartyId { get; set; } = string.Empty;
    public string? Tag { get; set; }
}
