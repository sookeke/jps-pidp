namespace Pidp.Infrastructure.HttpClients.Jum;

using Pidp.Models;

public interface IJumClient
{
    Task<JustinUser?> GetJumUserAsync(string username);
    Task<JustinUser?> GetJumUserByPartIdAsync(long partId);
    Task<bool> IsJumUser(JustinUser? justinUser, Party party);
}
