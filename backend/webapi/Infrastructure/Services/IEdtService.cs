namespace Pidp.Infrastructure.Services;

public interface IEdtService
{
    Task CreateUser(EdtUser user);
    Task<int> UpdateEmailLogStatuses(int limit);
}
