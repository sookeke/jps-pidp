namespace Pidp.Features.Organization.OrgUnitService;

using Pidp.Models;

public interface IOrgUnitService
{
    Task<IEnumerable<OrgUnitModel?>> GetUserOrgUnitGroup(int partyId, decimal participantId);
}
