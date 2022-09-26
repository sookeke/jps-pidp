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
}
