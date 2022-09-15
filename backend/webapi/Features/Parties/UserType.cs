namespace Pidp.Features.Parties;

using System.Threading.Tasks;
using FluentValidation;
using Pidp.Features.Organization.UserTypeService;
using Pidp.Models;

public class UserType
{
    public sealed record Query(int PartyId) : IQuery<UserTypeModel?>;
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator() => this.RuleFor(x => x.PartyId).GreaterThan(0);
    }
    public class QueryHandler : IQueryHandler<Query, UserTypeModel?>
    {
        private readonly IUserTypeService userType;

        public QueryHandler(IUserTypeService userType) => this.userType = userType;

        public async Task<UserTypeModel?> HandleAsync(Query query) => await this.userType.GetOrgUserType(query.PartyId);
    }
}
