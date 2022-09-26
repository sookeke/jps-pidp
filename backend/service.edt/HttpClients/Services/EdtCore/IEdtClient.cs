namespace edt.service.HttpClients.Services.EdtCore;

public interface IEdtClient
{
    Task<bool> CreateUser(EdtUserProvisioningModel accessRequest);
    Task<bool> UpdateUser(EdtUserProvisioningModel accessRequest);
    Task<EdtUserProvisioningModel?> GetUser(EdtUserProvisioningModel accessRequest);
}
