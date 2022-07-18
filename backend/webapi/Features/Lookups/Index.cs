namespace Pidp.Features.Lookups;

using Microsoft.EntityFrameworkCore;

using Pidp.Data;
using Pidp.Models.Lookups;

public class Index
{
    public class Query : IQuery<Model>
    {
    }

    public class Model
    {
        public List<College> Colleges { get; set; } = new();
        public List<Country> Countries { get; set; } = new();
        public List<Province> Provinces { get; set; } = new();
        public List<Organization> Organizations { get; set; } = new();
        public List<HealthAuthority> HealthAuthorities { get; set; } = new();
        public List<JusticeSector> JusticeSectors { get; set; } = new();
        public List<LawEnforcement> LawEnforcements { get; set; } = new();
        public List<CorrectionService> CorrectionServices { get; set; } = new();
        public List<LawSociety> LawSocieties { get; set; } = new();
    }

    public class QueryHandler : IQueryHandler<Query, Model>
    {
        private readonly PidpDbContext context;

        public QueryHandler(PidpDbContext context) => this.context = context;

        public async Task<Model> HandleAsync(Query query)
        {
            return new Model
            {
                Colleges = await this.context.Set<College>()
                    .ToListAsync(),
                Countries = await this.context.Set<Country>()
                    .ToListAsync(),
                Provinces = await this.context.Set<Province>()
                    .ToListAsync(),
                Organizations = await this.context.Set<Organization>()
                    .ToListAsync(),
                HealthAuthorities = await this.context.Set<HealthAuthority>()
                    .ToListAsync(),
                JusticeSectors = await this.context.Set<JusticeSector>()
                    .ToListAsync(),
                LawEnforcements = await this.context.Set<LawEnforcement>()
                    .ToListAsync(),
                CorrectionServices = await this.context.Set<CorrectionService>()
                    .ToListAsync(),
                LawSocieties = await this.context.Set<LawSociety>()
                    .ToListAsync()

            };
        }
    }
}
