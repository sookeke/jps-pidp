namespace Pidp.Infrastructure.HttpClients.Jum;

using NodaTime;

public class JustinUser
{
    public long UserId { get; set; }
    public string UserName { get; set; }
    public bool IsDisable { get; set; }
    public long ParticipantId { get; set; }
    public Guid? DigitalIdentifier { get; set; } = Guid.Empty;
    public Person person { get; set; }

    //public long AgencyId { get; set; }
    //public PartyTypeCode PartyTypeCode { get; set; }
    //public IEnumerable<RoleModel> Roles { get; set; }
}

public class Person
{
    public string FirstName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string MiddleNames { get; set; } = string.Empty;
    public string PreferredName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string? Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public enum PartyTypeCode
{
    Organization = 1,
    Individual = 2,
    Staff = 3,
}
