namespace edt.service.HttpClients.Services.EdtCore;

using System.Threading.Tasks;
using AutoMapper;
using Serilog;

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
            Log.Logger.Error("Failed to create EDT user {0}", string.Join(",", result.Errors));
            return false;
        }
        //add user to group
        var getUser = await this.GetUser(accessRequest.Key!);

        if (getUser != null)
        {
            var addGroupToUser = await this.AddUserGroups(getUser.Id!, accessRequest.AssignedRegions!);
            if (!addGroupToUser)
            {
                Log.Logger.Error("Failed to add EDT user to group user {0}", string.Join(",", result.Errors));

                return false;
            }
        }
        else
        {
            return false;
        }

        return result.IsSuccess;
    }
    public async Task<bool> AddUserGroups(string userIdOrKey, List<AssignedRegion> assignedRegions)
    {


        // Get existing groups assigned to user

        foreach (var region in assignedRegions)
        {

            Log.Logger.Information("Adding user {0} to region {1}", userIdOrKey, region);
            var groupId = await this.GetOuGroupId(region.RegionName!);
            if (groupId == 0)
            {
                return false;
            }

            var result = await this.PostAsync($"/api/v1/org-units/1/groups/{groupId}/users", new AddUserToOuGroup() { UserIdOrKey = userIdOrKey });

            if (!result.IsSuccess)
            {
                var successResult = false;
                Log.Logger.Error("Failed to add user {0} to region {1} due to {2}", userIdOrKey, region, string.Join(",", result.Errors));
                // we need a way to get existing groups - otherwise how do we keep the users/groups in-sync??
                foreach (var error in result.Errors)
                {
                    if (error.Contains("already a member of the group"))
                    {
                        Log.Logger.Information("User is already in group - ignoring error");
                        successResult = true;
                    }
                }
                return successResult;
            }
        }
        return true;

    }
    public async Task<bool> UpdateUser(EdtUserProvisioningModel accessRequest, EdtUserDto previousRequest)
    {

        Log.Logger.Information("Updating EDT User {0} {1}", accessRequest.ToString(), previousRequest.ToString());
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
            var addGroupToUser = await this.AddUserGroups(user.Id!, accessRequest.AssignedRegions!);
            if (!addGroupToUser)
            {
                return false;
            }
        }
        else
        {
            Log.Logger.Error("Failed to add user {0} to group {1}", accessRequest.Id, accessRequest.AssignedRegions);
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
