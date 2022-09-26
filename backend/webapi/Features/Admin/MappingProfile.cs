namespace Pidp.Features.Admin;

using AutoMapper;

using Pidp.Models;
using Pidp.Models.Lookups;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        this.CreateProjection<Party, PartyIndex.Model>()
            .ForMember(dest => dest.ProviderName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.ProviderOrganizationCode, opt => opt.MapFrom(src => src.OrgainizationDetail!.OrganizationCode))
            .ForMember(dest => dest.DigitalEvidenceAccessRequest, opt => opt.MapFrom(src => src.AccessRequests.Any(accessRequest => accessRequest.AccessTypeCode == AccessTypeCode.DigitalEvidence)))
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.OrgainizationDetail!.Organization!.Name));

    }
}
