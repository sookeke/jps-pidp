namespace Pidp.Features.Parties;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using HybridModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

using Pidp.Data;
using Pidp.Models.Lookups;
using Pidp.Infrastructure.HttpClients.Jum;
using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Identity;
using System.Globalization;
//using Pidp.Extensions;

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
            if (org != null && org.OrganizationCode == OrganizationCode.JusticeSector)
            {
                var details = await this.context.JusticeSectorDetails
                    .Where(detail => detail.OrgainizationDetail == org)
                    .Include(n => n.OrgainizationDetail)
                    .SingleOrDefaultAsync();

                if (details != null && orgDetails != null)
                {
                    orgDetails.JusticeSectorCode = details.JusticeSectorCode;
                    orgDetails.EmployeeIdentifier = details.JustinUserId;
                }

                orgDetails!.OrganizationCode = org.OrganizationCode;
            }
            if (org != null && org.OrganizationCode == OrganizationCode.CorrectionService)
            {
                var details = await this.context.CorrectionServiceDetails
                    .Where(detail => detail.OrgainizationDetail == org)
                    .Include(n => n.OrgainizationDetail)
                    .SingleOrDefaultAsync();

                if (details != null)
                {
                    orgDetails!.CorrectionServiceCode = details.CorrectionServiceCode;
                    orgDetails.EmployeeIdentifier = details.PeronalId;
                }

                orgDetails!.OrganizationCode = org.OrganizationCode;
            }

            return orgDetails ?? new Command { PartyId = query.PartyId };
        }
    }

    public class CommandHandler : ICommandHandler<Command>
    {
        private readonly PidpDbContext context;
        private readonly IJumClient jumClient;
        private readonly ILogger logger;
        private readonly IHttpContextAccessor httpContextAccessor;

        public CommandHandler(PidpDbContext context, IJumClient jumClient, ILogger<CommandHandler> logger, IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.jumClient = jumClient;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task HandleAsync(Command command)
        {
            var httpContext = this.httpContextAccessor.HttpContext;
            var accessToken = await httpContext!.GetTokenAsync("access_token");


            //accessToken.ThrowIfNull(accessToken);

            var dto = await this.context.Parties
                .Where(party => party.Id == command.PartyId)
                .SingleAsync();

            //var justinUser = new JustinUser();

            var justinUser = command.OrganizationCode switch
            {
                OrganizationCode.CorrectionService => await this.jumClient.GetJumUserByPartIdAsync(partId: long.Parse(command.EmployeeIdentifier, CultureInfo.InvariantCulture), accessToken!),
                OrganizationCode.JusticeSector => await this.jumClient.GetJumUserAsync(command.EmployeeIdentifier, accessToken!),
                OrganizationCode.LawEnforcement => throw new NotImplementedException(),
                OrganizationCode.LawSociety => throw new NotImplementedException(),
                OrganizationCode.HealthAuthority => throw new NotImplementedException(),
                OrganizationCode.BcGovernmentMinistry => throw new NotImplementedException(),
                OrganizationCode.ICBC => throw new NotImplementedException(),
                OrganizationCode.Other => throw new NotImplementedException(),
                _ => null
            };

            if (justinUser == null
                || dto.Email == null
                || !await this.jumClient.IsJumUser(justinUser, dto))
            {
                this.logger.LogInvalidJustinUser();
                //throw new ArgumentException("User not a valid Justin User.");
            }

            var org = await this.context.PartyOrgainizationDetails
                .SingleOrDefaultAsync(detail => detail.PartyId == command.PartyId);

            if (org == null)
            {
                org = new Models.PartyOrgainizationDetail
                {
                    PartyId = command.PartyId
                };
                this.context.PartyOrgainizationDetails.Add(org);
            }

            if (command.OrganizationCode == OrganizationCode.JusticeSector
                /*&& await this.jumClient.IsJumUser(justinUser, dto)*/)
            {
                var jpsDetail = await this.context.JusticeSectorDetails
               .SingleOrDefaultAsync(detail => detail.OrgainizationDetail == org) ?? null;

                if (jpsDetail == null)
                {
                    jpsDetail = new Models.JusticeSectorDetail
                    {
                        OrgainizationDetail = org,
                        JustinUserId = command.EmployeeIdentifier,
                        ParticipantId = justinUser is not null ? justinUser!.participantDetails[0].partId : string.Empty,
                        JusticeSectorCode = command.JusticeSectorCode
                    };
                    this.context.JusticeSectorDetails.Add(jpsDetail);
                }
                else
                {
                    jpsDetail.OrgainizationDetail = org;
                    jpsDetail.JustinUserId = command.EmployeeIdentifier;
                    jpsDetail.ParticipantId = justinUser is not null ? justinUser!.participantDetails[0].partId : string.Empty;
                    jpsDetail.JusticeSectorCode = command.JusticeSectorCode;
                    this.context.JusticeSectorDetails.Update(jpsDetail);
                }

                org.OrganizationCode = command.OrganizationCode;
                await this.context.SaveChangesAsync();
            }
            else if (command.OrganizationCode == OrganizationCode.CorrectionService
                /*&& await this.jumClient.IsJumUser(justinUser, dto)*/)
            {
                var corDetail = await this.context.CorrectionServiceDetails
                                    .SingleOrDefaultAsync(detail => detail.OrgainizationDetail == org);

                if (corDetail == null)
                {
                    corDetail = new Models.CorrectionServiceDetail
                    {
                        OrgainizationDetail = org,
                        PeronalId = command.EmployeeIdentifier,
                        CorrectionServiceCode = command.CorrectionServiceCode
                    };
                    this.context.CorrectionServiceDetails.Add(corDetail);
                }
                else
                {
                    corDetail.OrgainizationDetail = org;
                    corDetail.PeronalId = command.EmployeeIdentifier;
                    corDetail.CorrectionServiceCode = command.CorrectionServiceCode;
                    this.context.CorrectionServiceDetails.Update(corDetail);
                }

                org.OrganizationCode = command.OrganizationCode;
                await this.context.SaveChangesAsync();
            }
            else
            {
                org = new Models.PartyOrgainizationDetail
                {
                    PartyId = command.PartyId
                };
                this.context.PartyOrgainizationDetails.Add(org);

                org.OrganizationCode = command.OrganizationCode;
                await this.context.SaveChangesAsync();
            }
        }
    }
}
public static partial class JustinUserLoggingExtensions
{
    [LoggerMessage(1, LogLevel.Error, "User not a valid Justin User or does not meeting all prerequisites.")]
    public static partial void LogInvalidJustinUser(this ILogger logger);
}
