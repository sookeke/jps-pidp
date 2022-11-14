namespace Pidp.Features.AccessRequests;

internal class PartyDto
{
    public bool AlreadyEnroled { get; set; }
    public string? Cpn { get; set; }
    public string? Jpdid { get; set; }
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
}
