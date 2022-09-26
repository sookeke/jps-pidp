namespace Pidp.Features.Organization.UserTypeService;

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Pidp.Data;
using Pidp.Models;
using Pidp.Models.Lookups;

public class UserTypeService : IUserTypeService
{
    private readonly PidpDbContext context;
    public string? OrgUserType { get; set; }

    public UserTypeService(PidpDbContext context) => this.context = context;//this.OrgUserType = orgUserType;
    public async Task<UserTypeModel?> GetOrgUserType(int partyId)
    {
        var userType = new UserTypeModel();
        if (this.context.PartyOrgainizationDetails.ToListAsync().Result.Count == 0)
        {
            return null;
        }

        var orgCode = await this.context.PartyOrgainizationDetails
            .Where(p => p.PartyId == partyId)
            .FirstOrDefaultAsync();
        if (orgCode?.OrganizationCode == OrganizationCode.CorrectionService)
        {
            var corr = await this.context.CorrectionServiceDetails.SingleOrDefaultAsync(detail => detail.OrgainizationDetail == orgCode) ?? throw new KeyNotFoundException($"username {orgCode} not found");
            var corrCode = corr.CorrectionServiceCode switch
            {
                CorrectionServiceCode.OutofCustody => "Out of Custody",
                CorrectionServiceCode.Incustody => "In Custody User",
                CorrectionServiceCode.Both => throw new NotImplementedException(),
                _ => null
            };
            if (corrCode is not null)
            {
                this.OrgUserType = corrCode;
                userType = new UserTypeModel
                {
                    OrganizationType = nameof(OrganizationCode.CorrectionService),
                    OrganizationName = corrCode,
                    ParticipantId = corr.PeronalId
                };
                //.Add(nameof(OrganizationCode.CorrectionService), new Dictionary<string, string> { { "OrganizationName", corrCode }, { "ParticipantId", corr.PeronalId } });
            }
        }
        else if (orgCode?.OrganizationCode == OrganizationCode.JusticeSector)
        {
            var jsector = await this.context.JusticeSectorDetails.SingleOrDefaultAsync(detail => detail.OrgainizationDetail == orgCode) ?? throw new KeyNotFoundException($"username {orgCode} not found");
            var jsCode = jsector.JusticeSectorCode switch
            {
                JusticeSectorCode.BCPS => "BC Prosecution Service",
                JusticeSectorCode.RSBC => throw new NotImplementedException(),
                _ => null
            };
            if (jsCode is not null)
            {
                this.OrgUserType = jsCode;
                userType = new UserTypeModel
                {
                    OrganizationType = nameof(OrganizationCode.JusticeSector),
                    OrganizationName = jsCode,
                    ParticipantId = jsector.ParticipantId
                };
                //userType.Add(nameof(OrganizationCode.JusticeSector), new Dictionary<string, string> { { "OrganizationName", jsCode },{"ParticipantId", jsector.JustinUserId } });
            }
        }
        return userType is null ? throw new KeyNotFoundException() : userType;
    }
}
