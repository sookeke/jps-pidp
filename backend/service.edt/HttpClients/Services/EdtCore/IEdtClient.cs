namespace edt.service.HttpClients.Services.EdtCore;

public interface IEdtClient
{
    Task<bool> CreateUser(EdtUserProvisioningModel accessRequest);
    Task<bool> UpdateUser(EdtUserProvisioningModel accessRequest, EdtUserDto previousRequest);

    Task<int> GetOuGroupId(string regionName);

    Task<EdtUserDto?> GetUser(string userKey);

    /// <summary>
    /// Get EDT assigned groups for the user
    /// </summary>
    /// <param name="userKey">EDT user key</param>
    /// <returns></returns>
    Task<List<EdtUserGroup>> GetAssignedOUGroups(string userKey);

    /// <summary>
    /// Update the users groups in EDT
    /// This may involve adding new groups or removing non-assigned groups
    /// TODO - later this should be triggered from any change to the groups in JUSTIN
    /// </summary>
    /// <param name="userIdOrKey"></param>
    /// <param name="assignedRegions"></param>
    /// <returns></returns>
    Task<bool> UpdateUserAssignedGroups(string userIdOrKey, List<AssignedRegion> assignedRegions);

    /// <summary>
    /// Remove the user from the group
    /// </summary>
    /// <param name="userIdOrKey"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    Task<bool> RemoveUserFromGroup(string userIdOrKey, EdtUserGroup group);
}
