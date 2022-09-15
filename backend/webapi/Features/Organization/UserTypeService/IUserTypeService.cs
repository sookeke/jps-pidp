namespace Pidp.Features.Organization.UserTypeService;

using Pidp.Models;

public interface IUserTypeService
{
    Task<UserTypeModel?> GetOrgUserType(int partyId);
}
