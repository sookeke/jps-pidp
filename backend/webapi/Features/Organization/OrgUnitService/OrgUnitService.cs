namespace Pidp.Features.Organization.OrgUnitService;

using System.Threading.Tasks;
using Pidp.Infrastructure.HttpClients.Jum;
using Pidp.Data;
using Pidp.Models;
using Pidp.Models.Lookups;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using System.Diagnostics;
using Pidp.Infrastructure.Telemetry;

public class OrgUnitService : IOrgUnitService
{
    private readonly IJumClient jumClient;
    private readonly PidpDbContext context;
    private readonly IHttpContextAccessor httpContextAccessor;
    public OrgUnitService(IJumClient jumClient, PidpDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        this.jumClient = jumClient;
        this.context = context;
        this.httpContextAccessor = httpContextAccessor;
    }
    public async Task<IEnumerable<OrgUnitModel?>> GetUserOrgUnitGroup(int partyId, decimal participantId)
    {
        var httpContext = this.httpContextAccessor.HttpContext;
        var accessToken = await httpContext!.GetTokenAsync("access_token");

        using var activity = Telemetry.ActivitySource.StartActivity(TelemetryConstants.ServiceName + "-GetOrgUnitGroup");
        activity?.AddTag("digitalevidence.party.id", partyId);


        var dto = await this.context.Parties
                .Where(party => party.Id == partyId)
                .SingleAsync();



        var getJumUser = await this.jumClient.GetJumUserByPartIdAsync(participantId, accessToken!);
        if (getJumUser != null && getJumUser.participantDetails.Count > 0 && await this.jumClient.IsJumUser(getJumUser, dto))
        {
            var crownRegion = await this.context
                .Set<CrownRegion>()
                .ToListAsync();
            var assignedAgencies = getJumUser
                .participantDetails
                .FirstOrDefault()!
                .assignedAgencies
                .Select(n => n.agencyName.ToUpper(CultureInfo.CurrentCulture))
                .ToList();
            return crownRegion
                .Where(n => assignedAgencies.Contains(n.CrownLocation.ToUpper(CultureInfo.CurrentCulture)))
                .Select(orgUnit => new OrgUnitModel
                {
                    AssignedAgency = orgUnit.CrownLocation,
                    RegionName = orgUnit.RegionName,
                    RegionId = orgUnit.RegionId
                }).ToList();
        }
        return default!;
    }
}
