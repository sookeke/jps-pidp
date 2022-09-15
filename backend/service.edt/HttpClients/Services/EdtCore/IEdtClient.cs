namespace edt.service.HttpClients.Services.EdtCore;

using edt.service.HttpClients.Services.EdtCore;

public interface IEdtClient
{
    Task<bool> CreateUser(EdtUserProvisioningModel accessRequest);
}
