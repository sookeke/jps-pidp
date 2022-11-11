namespace edt.service.HttpClients.Services.EdtCore;

public interface IEdtClient
{
    Task<bool> CreateUser(EdtUserProvisioningModel accessRequest);
    Task<bool> UpdateUser(EdtUserProvisioningModel accessRequest);
    Task<bool> AddUserGroup(string userIdOrKey, List<string> regionName);
    Task<int> GetOuGroupId(string regionName);
    Task<EdtUserProvisioningModel?> GetUser(EdtUserProvisioningModel accessRequest);
}
