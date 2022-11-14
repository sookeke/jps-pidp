namespace edt.service.HttpClients.Services.EdtCore;

public interface IEdtClient
{
    Task<bool> CreateUser(EdtUserProvisioningModel accessRequest);
    Task<bool> UpdateUser(EdtUserProvisioningModel accessRequest, EdtUserDto previousRequest);
    Task<bool> AddUserGroup(string userIdOrKey, List<AssignedRegion> regionName);
    Task<int> GetOuGroupId(string regionName);
    Task<EdtUserDto?> GetUser(string userKey);
}
