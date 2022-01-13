namespace Pidp.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table(nameof(Party))]
public class Party : BaseAuditable
{
    [Key]
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? PreferredFirstName { get; set; }

    public string? PreferredMiddleName { get; set; }

    public string? PreferredLastName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public PartyCertification? PartyCertification { get; set; }

    public string? JobTitle { get; set; }

    public Facility? Facility { get; set; }
}
