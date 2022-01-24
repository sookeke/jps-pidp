namespace Pidp.Features.Parties;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using DomainResults.Common;
using FluentValidation;
using HybridModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;

using Pidp.Data;
using Pidp.Models.Lookups;

public class ProfileStatus
{
    public class Query : IQuery<IDomainResult<Model>>
    {
        public int Id { get; set; }
    }

    public class Model
    {
        [HybridBindProperty(Source.Route)]
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public LocalDate Birthdate { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public CollegeCode? CollegeCode { get; set; }
        public string? LicenceNumber { get; set; }
        public string? JobTitle { get; set; }
        public string? FacilityName { get; set; }
        public string? FacilityStreet { get; set; }

        public bool DemographicsComplete => this.Email != null && this.Phone != null;
        public bool CollegeCertificationComplete => this.CollegeCode != null && this.LicenceNumber != null;
        public bool WorkSettingComplete => this.JobTitle != null && this.FacilityName != null;
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator() => this.RuleFor(x => x.Id).GreaterThan(0);
    }

    public class QueryHandler : IQueryHandler<Query, IDomainResult<Model>>
    {
        private readonly PidpDbContext context;
        private readonly IMapper mapper;

        public QueryHandler(PidpDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<IDomainResult<Model>> HandleAsync(Query query)
        {
            var model = await this.context.Parties
                .Where(party => party.Id == query.Id)
                .ProjectTo<Model>(this.mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();

            if (model == null)
            {
                return DomainResult.NotFound<Model>();
            }

            return DomainResult.Success(model);
        }
    }
}
