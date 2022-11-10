namespace Pidp.Features.Parties;

using FluentValidation;
using Pidp.Features.Organization.OrgUnitService;
using Pidp.Models;

public class CrownRegionQuery
{
    public sealed record Query(int PartyId, decimal ParticipantId) : IQuery<IEnumerable<OrgUnitModel?>>;
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            this.RuleFor(x => x.ParticipantId).GreaterThan(0);
            this.RuleFor(x => x.PartyId).GreaterThan(0);
        }
    }
    public class QueryHandler : IQueryHandler<Query, IEnumerable<OrgUnitModel?>>
    {
        private readonly IOrgUnitService orgUnitService;

        public QueryHandler(IOrgUnitService orgUnitService) => this.orgUnitService = orgUnitService;

        public async Task<IEnumerable<OrgUnitModel?>> HandleAsync(Query query) => await this.orgUnitService.GetUserOrgUnitGroup(query.PartyId, query.ParticipantId);
    }
}
