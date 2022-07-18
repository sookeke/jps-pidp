namespace Pidp.Features.Parties;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using HybridModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

using Pidp.Data;
using Pidp.Models.Lookups;

public class OrganizationDetails
{
    public class Query : IQuery<Command>
    {
        public int PartyId { get; set; }
    }

    public class Command : ICommand
    {
        [JsonIgnore]
        [HybridBindProperty(Source.Route)]
        public int PartyId { get; set; }

        public OrganizationCode OrganizationCode { get; set; }
        public HealthAuthorityCode? HealthAuthorityCode { get; set; }
        public JusticeSectorCode? JusticeSectorCode { get; set; }
        public CorrectionServiceCode? CorrectionServiceCode { get; set; }
        public string EmployeeIdentifier { get; set; } = string.Empty;
    }

    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator() => this.RuleFor(x => x.PartyId).GreaterThan(0);
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            this.RuleFor(x => x.PartyId).GreaterThan(0);
            this.RuleFor(x => x.OrganizationCode).IsInEnum();
            //this.RuleFor(x => x.HealthAuthorityCode).IsInEnum();
            this.RuleFor(x => x.EmployeeIdentifier).NotEmpty();
        }
    }

    public class QueryHandler : IQueryHandler<Query, Command>
    {
        private readonly IMapper mapper;
        private readonly PidpDbContext context;

        public QueryHandler(IMapper mapper, PidpDbContext context)
        {
            this.mapper = mapper;
            this.context = context;
        }

        public async Task<Command> HandleAsync(Query query)
        {
            var orgDetails = await this.context.PartyOrgainizationDetails
                .Where(detail => detail.PartyId == query.PartyId)
                .ProjectTo<Command>(this.mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
            var org = await this.context.PartyOrgainizationDetails
                .Where(detail => detail.PartyId == query.PartyId)
                //.ProjectTo<Command>(this.mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
            if (org != null  && org.OrganizationCode == OrganizationCode.JusticeSector)
            {
                var details = await this.context.JusticeSectorDetails
                    .Where(detail => detail.OrgainizationDetail == org)
                    .Include(n => n.OrgainizationDetail)
                    .SingleOrDefaultAsync();

                orgDetails.JusticeSectorCode = details.JusticeSectorCode;
                orgDetails.EmployeeIdentifier = details.JustinUserId;
                orgDetails.OrganizationCode = org.OrganizationCode;
            }
            if (org != null && org.OrganizationCode == OrganizationCode.CorrectionService)
            {
                var details = await this.context.CorrectionServiceDetails
                    .Where(detail => detail.OrgainizationDetail == org)
                    .Include(n => n.OrgainizationDetail)
                    .SingleOrDefaultAsync();

                orgDetails.CorrectionServiceCode = details.CorrectionServiceCode;
                orgDetails.EmployeeIdentifier = details.PeronalId;
                orgDetails.OrganizationCode = org.OrganizationCode;
            }

            return orgDetails ?? new Command { PartyId = query.PartyId };
        }
    }

    public class CommandHandler : ICommandHandler<Command>
    {
        private readonly PidpDbContext context;

        public CommandHandler(PidpDbContext context) => this.context = context;

        public async Task HandleAsync(Command command)
        {
            var org = await this.context.PartyOrgainizationDetails
                .SingleOrDefaultAsync(detail => detail.PartyId == command.PartyId);

            var jpsDetail = await this.context.JusticeSectorDetails
               .SingleOrDefaultAsync(detail => detail.OrgainizationDetail == org);


            if (org == null && command.OrganizationCode == OrganizationCode.JusticeSector)
            {
                org = new Models.PartyOrgainizationDetail
                {
                    PartyId = command.PartyId
                };
                jpsDetail = new Models.JusticeSectorDetail
                {
                    OrgainizationDetail = org,
                    JustinUserId = command.EmployeeIdentifier,
                    JusticeSectorCode = (JusticeSectorCode)command.JusticeSectorCode
                };
                this.context.PartyOrgainizationDetails.Add(org);
                this.context.JusticeSectorDetails.Add(jpsDetail);
            }
            else if (org == null && command.OrganizationCode == OrganizationCode.CorrectionService)
            {
                var corDetail = await this.context.CorrectionServiceDetails
                                    .SingleOrDefaultAsync(detail => detail.OrgainizationDetail == org);

                org = new Models.PartyOrgainizationDetail
                {
                    PartyId = command.PartyId
                };
                corDetail = new Models.CorrectionServiceDetail
                {
                    OrgainizationDetail = org,
                    PeronalId = command.EmployeeIdentifier,
                    CorrectionServiceCode = (CorrectionServiceCode)command.CorrectionServiceCode
                };
                this.context.PartyOrgainizationDetails.Add(org);
                this.context.CorrectionServiceDetails.Add(corDetail);
            }
            else
            {
                org = new Models.PartyOrgainizationDetail
                {
                    PartyId = command.PartyId
                };
                this.context.PartyOrgainizationDetails.Add(org);
            }

            org.OrganizationCode = command.OrganizationCode;
            //org.HealthAuthorityCode = command.HealthAuthorityCode;
            //org.EmployeeIdentifier = command.EmployeeIdentifier;

            await this.context.SaveChangesAsync();
        }
    }
}
