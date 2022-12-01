namespace edt.service.HttpClients.Services.EdtCore;

using System.Threading.Tasks;
using AutoMapper;
using DomainResults.Common;
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
            var addGroupToUser = await this.UpdateUserAssignedGroups(getUser.Id!, accessRequest.AssignedRegions!);
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
    public async Task<bool> UpdateUserAssignedGroups(string userIdOrKey, List<AssignedRegion> assignedRegions)
    {

        // Get existing groups assigned to user
        var currentlyAssignedGroups = await this.GetAssignedOUGroups(userIdOrKey);

        foreach ( var currentAssignedGroup in currentlyAssignedGroups)
        {
            AssignedRegion assignedRegion = assignedRegions.Find(region => region.RegionName.Equals(currentAssignedGroup.Name))!;
            if ( assignedRegion == null)
            {
                Log.Logger.Information("User {0} is in group {1} that is no longer valid", userIdOrKey, currentAssignedGroup.Name);
                var result = await this.RemoveUserFromGroup(userIdOrKey, currentAssignedGroup);
                if (result)
                {
                    return false;
                }
            }
        }


        foreach (var region in assignedRegions)
        {

            EdtUserGroup? existingGroup = currentlyAssignedGroups.Find(g => g.Name.Equals(region.RegionName));

            if (existingGroup != null)
            {
                Log.Logger.Information("User {0} already assigned to group {1}", userIdOrKey, existingGroup.Name);
            }
            else
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
                    Log.Logger.Error("Failed to add user {0} to region {1} due to {2}", userIdOrKey, region, string.Join(",", result.Errors));
                    return false;                  
                }
            }
        }

        Log.Logger.Information("User {0} group synchronization completed");
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
            var addGroupToUser = await this.UpdateUserAssignedGroups(user.Id!, accessRequest.AssignedRegions!);
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
        Log.Logger.Information("Checking if user key {0} already present", userKey);
        var result = await this.GetAsync<EdtUserDto?>($"/api/v1/users/key:{userKey}");

        if (!result.IsSuccess)
        {
            return null;
        }
        return result.Value;
    }

    public async Task<int> GetOuGroupId(string regionName)
    {
        IDomainResult<IEnumerable<OrgUnitModel?>>? result = await this.GetAsync<IEnumerable<OrgUnitModel?>>($"/api/v1/org-units/1/groups");

        if (!result.IsSuccess)
        {
            return 0; //invalid
        }

        return result.Value!
            .Where(ou => ou!.Name == regionName)
            .Select(ou => ou!.Id)
            .FirstOrDefault();
    }

    public async Task<List<EdtUserGroup>> GetAssignedOUGroups(string userKey)
    {
        IDomainResult<List<EdtUserGroup>?>? result = await this.GetAsync<List<EdtUserGroup>?>($"/api/v1/org-units/1/users/{userKey}/groups");
        if (!result.IsSuccess)
        {
            Log.Logger.Error("Failed to determine existing EDT groups for {0} [{1}]", string.Join(", ", result.Errors));
            return null; //invalid
        }

        return result.Value;


    }

    public async Task<bool> RemoveUserFromGroup(string userIdOrKey, EdtUserGroup group)
    {
        Log.Logger.Information("Removing user {0} from group {1}", userIdOrKey, group.Name);
        var result = await this.DeleteAsync($"/api/v1/org-units/1/groups/{group.Id}/users/{userIdOrKey}");
        if (!result.IsSuccess)
        {
            Log.Logger.Error("Failed to remove user {0} from group {1} [{2}]", userIdOrKey, group.Name, string.Join(',', result.Errors));
            return false;
        }

        return true;
    }

    public class AddUserToOuGroup
    {
        public string? UserIdOrKey { get; set; }
    }
}
