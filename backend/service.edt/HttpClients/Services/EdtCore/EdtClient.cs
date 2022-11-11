namespace edt.service.HttpClients.Services.EdtCore;

using System.Threading.Tasks;


public class EdtClient : BaseClient, IEdtClient
{

    public EdtClient(
        HttpClient httpClient,
        ILogger<EdtClient> logger)
        : base(httpClient, logger) { }

    public async Task<bool> CreateUser(EdtUserProvisioningModel accessRequest)
    {
        var result = await this.PostAsync($"/api/v1/users", accessRequest);

        if (!result.IsSuccess)
        {
            return false;
        }
        return result.IsSuccess;
    }
    public async Task<bool> AddUserGroup(string userIdOrKey, List<string> regionName)
    {
        foreach (var region in regionName)
        {
            var groupId = await this.GetOuGroupId(region);

            var result = await this.PostAsync($"/api/v1/org-units/1/groups/{groupId}/users", userIdOrKey);

            if (!result.IsSuccess)
            {
                return false;
            }
        }
        return true;

    }
    public async Task<bool> UpdateUser(EdtUserProvisioningModel accessRequest)
    {
        var result = await this.PutAsync($"/api/v1/users", accessRequest);

        if (!result.IsSuccess)
        {
            return false;
        }
        return result.IsSuccess;
    }
    public async Task<EdtUserProvisioningModel?> GetUser(EdtUserProvisioningModel accessRequest)
    {
        var result = await this.GetAsync<EdtUserProvisioningModel?>($"/api/v1/users/key:{accessRequest.Key}");

        if (!result.IsSuccess)
        {
            return null;
        }
        return result.Value;
    }

    public async Task<int> GetOuGroupId(string regionName)
    {
        var result = await this.GetAsync<IEnumerable<OrgUnitModel?>>($"/api/v1/org-units/1/groups");

        if (!result.IsSuccess)
        {
            throw new Exception(); //handle the exception via middleware error handler
        }

        return result.Value!
            .Where(ou => ou!.Name == regionName)
            .Select(ou => ou!.Id)
            .FirstOrDefault();
    }
}
