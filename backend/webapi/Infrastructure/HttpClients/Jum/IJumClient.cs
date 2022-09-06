namespace Pidp.Infrastructure.HttpClients.Jum;

using Pidp.Models;

public interface IJumClient
{
    Task<JustinUser?> GetJumUserAsync(string username);
    Task<Participant?> GetJumUserAsync(string username, string accessToken);
    Task<JustinUser?> GetJumUserByPartIdAsync(long partId);
    Task<Participant?> GetJumUserByPartIdAsync(decimal partId, string accessToken);
    Task<bool> IsJumUser(Participant? justinUser, Party party);
}
