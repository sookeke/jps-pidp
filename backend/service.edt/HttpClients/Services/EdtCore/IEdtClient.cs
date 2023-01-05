namespace edt.service.HttpClients.Services.EdtCore;

using edt.service.Kafka.Model;
using edt.service.ServiceEvents.UserAccountCreation.Models;

public interface IEdtClient
{
    Task<UserModificationEvent> CreateUser(EdtUserProvisioningModel accessRequest);
    Task<UserModificationEvent> UpdateUser(EdtUserProvisioningModel accessRequest, EdtUserDto previousRequest);

    Task<int> GetOuGroupId(string regionName);

    Task<EdtUserDto?> GetUser(string userKey);

    /// <summary>
    /// Get the version of EDT (also acts as a simple ping test)
    /// </summary>
    /// <returns></returns>
    Task<string> GetVersion();

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
    Task<bool> UpdateUserAssignedGroups(string userIdOrKey, List<AssignedRegion> assignedRegions, UserModificationEvent userModificationEvent);

    /// <summary>
    /// Remove the user from the group
    /// </summary>
    /// <param name="userIdOrKey"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    Task<bool> RemoveUserFromGroup(string userIdOrKey, EdtUserGroup group);


    ///

}
