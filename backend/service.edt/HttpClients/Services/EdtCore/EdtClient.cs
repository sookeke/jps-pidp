namespace edt.service.HttpClients.Services.EdtCore;

using System.Threading.Tasks;
using AutoMapper;

public class EdtClient : BaseClient, IEdtClient
{
    private readonly IMapper mapper;
    public EdtClient(
        HttpClient httpClient,
        IMapper mapper,
        ILogger<EdtClient> logger)
        : base(httpClient, logger) => this.mapper = mapper;

    public async Task<bool> CreateUser(EdtUserProvisioningModel accessRequest)
    {
        var edtUserDto = this.mapper.Map<EdtUserProvisioningModel, EdtUserDto>(accessRequest);
        var result = await this.PostAsync($"/api/v1/users", edtUserDto);

        if (!result.IsSuccess)
        {
            return false;
        }
        //add user to group
        var getUser = await this.GetUser(accessRequest.Key!);
        if (getUser != null)
        {
            var addGroupToUser = await this.AddUserGroup(getUser.Id!, accessRequest.AssignedRegion!);
            if (!addGroupToUser)
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        return result.IsSuccess;
    }
    public async Task<bool> AddUserGroup(string userIdOrKey, List<AssignedRegion> regionName)
    {
        foreach (var region in regionName)
        {
            var groupId = await this.GetOuGroupId(region.RegionName!);
            if (groupId == 0)
            {
                return false;
            }

            var result = await this.PostAsync($"/api/v1/org-units/1/groups/{groupId}/users", new AddUserToOuGroup() { UserIdOrKey = userIdOrKey });

            if (!result.IsSuccess)
            {
                return false;
            }
        }
        return true;

    }
    public async Task<bool> UpdateUser(EdtUserProvisioningModel accessRequest, EdtUserDto previousRequest)
    {
        var edtUserDto = this.mapper.Map<EdtUserProvisioningModel, EdtUserDto>(accessRequest);
        edtUserDto.Id = previousRequest.Id;
        var result = await this.PutAsync($"/api/v1/users", edtUserDto);

        if (!result.IsSuccess)
        {
            return false;
        }
        //add user to group
        var user = await this.GetUser(accessRequest.Key!);
        if (user != null)
        {
            var addGroupToUser = await this.AddUserGroup(user.Id!, accessRequest.AssignedRegion!);
            if (!addGroupToUser)
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        return result.IsSuccess;
    }
    public async Task<EdtUserDto?> GetUser(string userKey)
    {
        var result = await this.GetAsync<EdtUserDto?>($"/api/v1/users/key:{userKey}");

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
            return 0; //invalid
        }

        return result.Value!
            .Where(ou => ou!.Name == regionName)
            .Select(ou => ou!.Id)
            .FirstOrDefault();
    }

    public class AddUserToOuGroup
    {
        public string? UserIdOrKey { get; set; }
    }
}
