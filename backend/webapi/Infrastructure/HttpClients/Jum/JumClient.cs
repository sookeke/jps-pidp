namespace Pidp.Infrastructure.HttpClients.Jum;

using System.Threading.Tasks;
using NodaTime;
using Pidp.Models;

public class JumClient : BaseClient, IJumClient
{
    public JumClient(HttpClient httpClient, ILogger<JumClient> logger) : base(httpClient, logger) { }

    public async Task<JustinUser?> GetJumUserAsync(string username)
    {
        var result = await this.GetAsync<JustinUser>($"users/{username}");

        if (!result.IsSuccess)
        {
            return null;
        }
        var user = result.Value;
        if (user == null)
        {
            this.Logger.LogNoUserFound(username);
            return null;
        }
        if (user.IsDisable)
        {
            this.Logger.LogDisabledUserFound(username);
            return null;
        }
        return user;
    }

    public async Task<JustinUser?> GetJumUserByPartIdAsync(long partId)
    {
        var result = await this.GetAsync<JustinUser>($"users/{partId}");

        if (!result.IsSuccess)
        {
            return null;
        }
        var user = result.Value;
        if (user == null)
        {
            this.Logger.LogNoUserWithPartIdFound(partId);
            return null;
        }
        if (user.IsDisable)
        {
            this.Logger.LogDisabledPartIdFound(partId);
            return null;
        }
        return user;
    }

    public Task<bool> IsJumUser(JustinUser? justinUser, Party party)
    {
        if (justinUser == null
            || party == null)
        {
            this.Logger.LogJustinUserNotFound();
            return Task.FromResult(false);
        }

        if (justinUser.person.FirstName == party.FirstName
            && justinUser.person.Surname == party.LastName
            && justinUser.person.Email == party.Email
            && !justinUser.IsDisable
            && LocalDate.FromDateTime(justinUser.person.BirthDate) == party.Birthdate
            && justinUser.person.Gender == party.Gender)
        {
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
public static partial class JumClientLoggingExtensions
{
    [LoggerMessage(1, LogLevel.Warning, "No User found in JUM with Username = {username}.")]
    public static partial void LogNoUserFound(this ILogger logger, string username);
    [LoggerMessage(2, LogLevel.Warning, "No User found in JUM with PartId = {partId}.")]
    public static partial void LogNoUserWithPartIdFound(this ILogger logger, long partId);
    [LoggerMessage(3, LogLevel.Warning, "User found but disabled in JUM with Username = {username}.")]
    public static partial void LogDisabledUserFound(this ILogger logger, string username);
    [LoggerMessage(4, LogLevel.Warning, "User found but disabled in JUM with PartId = {partId}.")]
    public static partial void LogDisabledPartIdFound(this ILogger logger, long partId);
    [LoggerMessage(5, LogLevel.Error, "Justin user not found.")]
    public static partial void LogJustinUserNotFound(this ILogger logger);
}
