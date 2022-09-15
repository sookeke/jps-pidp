namespace edt.service.HttpClients.Services.EdtCore;

using System.Threading.Tasks;

public class EdtClient : BaseClient, IEdtClient
{
    private readonly string apiKey;
    public EdtClient(
        HttpClient httpClient,
        ILogger<EdtClient> logger,
        EdtServiceConfiguration config)
        : base(httpClient, logger) => this.apiKey = config.EdtClient.ApiKey;

    public async Task<bool> CreateUser(EdtUserProvisioningModel accessRequest)
    {

        var result = await this.PostAsync($"/api/v1/users", accessRequest);

        if (!result.IsSuccess)
        {
            return false;
        }
        return result.IsSuccess;
    }
}
