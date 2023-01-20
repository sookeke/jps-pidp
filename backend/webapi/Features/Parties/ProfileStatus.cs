namespace Pidp.Features.Parties;

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Pidp.Data;
using Pidp.Extensions;
using Pidp.Infrastructure;
using Pidp.Infrastructure.Auth;
using Pidp.Infrastructure.HttpClients.Plr;
using Pidp.Models.Lookups;
using Pidp.Infrastructure.HttpClients.Jum;
using Pidp.Models;
using static Pidp.Features.Parties.ProfileStatus.ProfileStatusDto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

public partial class ProfileStatus
{
    public class Command : ICommand<Model>
    {
        public int Id { get; set; }
        [JsonIgnore]
        public ClaimsPrincipal? User { get; set; }
        public Command WithUser(ClaimsPrincipal user)
        {
            this.User = user;
            return this;
        }
    }

    public partial class Model
    {
        [JsonConverter(typeof(PolymorphicDictionarySerializer<string, ProfileSection>))]
        public Dictionary<string, ProfileSection> Status { get; set; } = new();
        public HashSet<Alert> Alerts => new(this.Status.SelectMany(x => x.Value.Alerts));

        public abstract class ProfileSection
        {
            internal abstract string SectionName { get; }
            public HashSet<Alert> Alerts { get; set; } = new();
            public StatusCode StatusCode { get; set; }

            public bool IsComplete => this.StatusCode == StatusCode.Complete;

            public ProfileSection(ProfileStatusDto profile) => this.SetAlertsAndStatus(profile);

            protected abstract void SetAlertsAndStatus(ProfileStatusDto profile);
        }

        public enum Alert
        {
            TransientError = 1,
            PlrBadStanding,
            JumValidationError,
            PendingRequest
        }

        public enum StatusCode
        {
            Incomplete = 1,
            Complete,
            Locked,
            Error,
            Hidden,
            Pending
        }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator() => this.RuleFor(x => x.Id).GreaterThan(0);
    }

    public class CommandHandler : ICommandHandler<Command, Model>
    {
        private readonly IMapper mapper;
        private readonly IPlrClient client;
        private readonly IJumClient jumClient;
        private readonly PidpDbContext context;
        private readonly IHttpContextAccessor httpContextAccessor;

        public CommandHandler(
            IMapper mapper,
            IPlrClient client,
            IJumClient jumClient,
            PidpDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            this.mapper = mapper;
            this.client = client;
            this.context = context;
            this.jumClient = jumClient;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<Model> HandleAsync(Command command)
        {
            var profile = await this.context.Parties
               .Where(party => party.Id == command.Id)
               .ProjectTo<ProfileStatusDto>(this.mapper.ConfigurationProvider)
               .SingleAsync();

            var orgCorrectionDetail = profile.OrganizationCode == OrganizationCode.CorrectionService
                ? await this.context.CorrectionServiceDetails
                .Include(cor => cor.CorrectionService)
                .Where(detail => detail.OrgainizationDetail.Id == profile.OrgDetailId)
                .AsSplitQuery()
                .FirstOrDefaultAsync()
                : null;

            var orgJusticeSecDetail = profile.OrganizationCode == OrganizationCode.JusticeSector
                ? await this.context.JusticeSectorDetails
                .Include(jus => jus.JusticeSector)
                .Where(detail => detail.OrgainizationDetail.Id == profile.OrgDetailId)
                .AsSplitQuery()
                .FirstOrDefaultAsync()
                : null;

    


            //if (profile.CollegeCertificationEntered && profile.Ipc == null)
            if (profile.HasDeclaredLicence
                && string.IsNullOrWhiteSpace(profile.Cpn))
            {
                // Cert has been entered but no CPN found, likely due to a transient error or delay in PLR record updates. Retry once.
                profile.Cpn = await this.RecheckCpn(command.Id, profile.LicenceDeclaration, profile.Birthdate);
            }

            // if the user is a BCPS user then we'll flag this portion as completed
            if (profile.OrganizationDetailEntered && profile.OrganizationCode == OrganizationCode.CorrectionService && orgCorrectionDetail != null)
            {
                //get user token
                var httpContext = this.httpContextAccessor.HttpContext;
                var accessToken = await httpContext!.GetTokenAsync("access_token");
                profile.EmployeeIdentifier = orgCorrectionDetail.PeronalId;
                profile.CorrectionServiceCode = orgCorrectionDetail.CorrectionServiceCode;
                profile.CorrectionService = orgCorrectionDetail.CorrectionService?.Name;
                //profile.Organization = profile.or
                profile.JustinUser = await this.jumClient.GetJumUserByPartIdAsync(long.Parse(profile.EmployeeIdentifier, CultureInfo.InvariantCulture), accessToken!);
                profile.IsJumUser = await this.jumClient.IsJumUser(profile.JustinUser, new Party
                {
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    Email = profile.Email,
                    Birthdate = profile.Birthdate,
                    Gender = profile.Gender
                });

            }

            if (profile.OrganizationDetailEntered && profile.OrganizationCode == OrganizationCode.JusticeSector && orgJusticeSecDetail != null)
            {
                var accessToken = await this.httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                profile.EmployeeIdentifier = orgJusticeSecDetail.JustinUserId;
                profile.JusticeSectorCode = orgJusticeSecDetail.JusticeSectorCode;
                profile.JusticeSectorService = orgJusticeSecDetail.JusticeSector?.Name;
                profile.JustinUser = await this.jumClient.GetJumUserAsync(profile.EmployeeIdentifier, accessToken: accessToken!.ToString());
                profile.IsJumUser = await this.jumClient.IsJumUser(profile.JustinUser, new Party
                {
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    Email = profile.Email,
                    Birthdate = profile.Birthdate,
                    Gender = profile.Gender
                });
            }

            //profile.PlrRecordStatus = await this.client.GetRecordStatus(profile.Ipc);
            profile.PlrStanding = await this.client.GetStandingsDigestAsync(profile.Cpn);
             profile.User = command.User;

            // if the user is not a card user then we shouldnt need more profile info
            if (!profile.UserIsBcServicesCard )
            {
                // get the party
                var party = await this.context.Parties
               .SingleAsync(party => party.Id == command.Id);

                if (party != null)
                {
                    profile.Email = party.Email;
                }
            }

            var profileStatus = new Model
            {
                Status = new List<Model.ProfileSection>
                {
                    new Model.AccessAdministrator(profile),
                    new Model.CollegeCertification(profile),
                    new Model.OrganizationDetails(profile),
                    new Model.Demographics(profile),
                    new Model.DriverFitness(profile),
                    new Model.HcimAccountTransfer(profile),
                    new Model.HcimEnrolment(profile),
                    new Model.DigitalEvidence(profile),
                    new Model.MSTeams(profile),
                    new Model.SAEforms(profile),
                    new Model.Uci(profile)
                }
                .ToDictionary(section => section.SectionName, section => section)
            };

            return profileStatus;
        }

        private async Task<string?> RecheckCpn(int partyId, LicenceDeclarationDto declaration, LocalDate? birthdate)
        {
            if (declaration.HasNoLicence
                || birthdate == null)
            {
                return null;
            }

            var newCpn = await this.client.FindCpnAsync(declaration.CollegeCode.Value, declaration.LicenceNumber, birthdate.Value);
            if (newCpn != null)
            {
                var party = await this.context.Parties
                    .SingleAsync(party => party.Id == partyId);
                party.Cpn = newCpn;
                await this.context.SaveChangesAsync();
            }

            return newCpn;
        }
        private async Task<JustinUser?> RecheckJustinUser(OrganizationCode organizationCode, string personalId)
        {
            var newUser = new JustinUser();
            if (organizationCode == OrganizationCode.CorrectionService)
            {
                newUser = await this.jumClient.GetJumUserByPartIdAsync(long.Parse(personalId));
            }
            else if (organizationCode == OrganizationCode.JusticeSector)
            {
                newUser = await this.jumClient.GetJumUserAsync(personalId);
            }

            return newUser;
        }
    }


    public class ProfileStatusDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public LocalDate? Birthdate { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Phone { get; set; }
        public string? Cpn { get; set; }
        public LicenceDeclarationDto? LicenceDeclaration { get; set; }
        public string? AccessAdministratorEmail { get; set; }
        public CollegeCode? CollegeCode { get; set; }
        public string? LicenceNumber { get; set; }
        public string? Ipc { get; set; }
        public int? OrgDetailId { get; set; }
        public OrganizationCode? OrganizationCode { get; set; }
        public Organization? Organization { get; set; }
        public CorrectionServiceCode? CorrectionServiceCode { get; set; }
        public string? CorrectionService { get; set; }
        public JusticeSectorCode? JusticeSectorCode { get; set; }
        public string? JusticeSectorService { get; set; }
        public string? EmployeeIdentifier { get; set; }
        //public bool OrganizationDetailEntered { get; set; }
        public IEnumerable<AccessTypeCode> CompletedEnrolments { get; set; } = Enumerable.Empty<AccessTypeCode>();
        public IEnumerable<string> AccessRequestStatus { get; set; } = Enumerable.Empty<string>();


        // Resolved after projection
        public PlrStandingsDigest PlrStanding { get; set; } = default!;
        public ClaimsPrincipal? User { get; set; }

        public HttpContextAccessor? HttpContextAccessor { get; set; }

        public Participant? JustinUser { get; set; }
        public bool IsJumUser { get; set; }

        // Computed Properties
        [MemberNotNullWhen(true, nameof(Email), nameof(Phone))]
        public bool DemographicsEntered => this.User.GetIdentityProvider() == ClaimValues.Bcps || this.User.GetIdentityProvider() == ClaimValues.Idir || this.User.GetIdentityProvider() == ClaimValues.Adfs ? this.Email != null : this.Email != null && this.Phone != null;
        [MemberNotNullWhen(true, nameof(CollegeCode), nameof(LicenceNumber))]
        public bool CollegeCertificationEntered => this.CollegeCode.HasValue && this.LicenceNumber != null;
        [MemberNotNullWhen(true, nameof(OrganizationCode), nameof(EmployeeIdentifier))]
        public bool OrganizationDetailEntered => this.OrganizationCode.HasValue || this.EmployeeIdentifier != null;
        [MemberNotNullWhen(true, nameof(Organization))]
        public string? OrgName => this.Organization?.Name;
        public bool UserIsBcServicesCard => this.User.GetIdentityProvider() == ClaimValues.BCServicesCard;
        public bool UserIsPhsa => this.User.GetIdentityProvider() == ClaimValues.Phsa;
        //public bool UserIsBcps => this.User.GetIdentityProvider() == ClaimValues.Bcps;
        public bool UserIsBcps => this.User.GetIdentityProvider() == ClaimValues.Bcps && this.User?.Identity is ClaimsIdentity identity && identity.GetResourceAccessRoles(Clients.PidpApi).Contains(DefaultRoles.Bcps);
        public bool UserIsIdir => this.User.GetIdentityProvider() == ClaimValues.Idir;
        [MemberNotNullWhen(true, nameof(LicenceDeclaration))]
        public bool HasDeclaredLicence => this.LicenceDeclaration?.HasNoLicence == false;

        public class LicenceDeclarationDto
        {
            public CollegeCode? CollegeCode { get; set; }
            public string? LicenceNumber { get; set; }

            [MemberNotNullWhen(false, nameof(CollegeCode), nameof(LicenceNumber))]
            public bool HasNoLicence => this.CollegeCode == null || this.LicenceNumber == null;
        }
    }
}
