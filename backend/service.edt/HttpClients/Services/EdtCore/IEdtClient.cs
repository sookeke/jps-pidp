namespace edt.service.HttpClients.Services.EdtCore;

public interface IEdtClient
{
    Task<bool> CreateUser(EdtUserProvisioningModel accessRequest);
    Task<bool> UpdateUser(EdtUserProvisioningModel accessRequest, EdtUserDto previousRequest);
    Task<bool> AddUserGroups(string userIdOrKey, List<AssignedRegion> assignedRegions);
    Task<int> GetOuGroupId(string regionName);
    Task<EdtUserDto?> GetUser(string userKey);
}
