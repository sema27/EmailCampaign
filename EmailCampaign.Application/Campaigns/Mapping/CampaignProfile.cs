using AutoMapper;
using EmailCampaign.Application.Campaigns.Dtos.Requests;
using EmailCampaign.Application.Campaigns.Dtos.Responses;
using EmailCampaign.Application.Campaigns.Messaging;
using EmailCampaign.Domain.Entities;

namespace EmailCampaign.Application.Campaigns.Mapping;

public sealed class CampaignProfile : Profile
{
    public CampaignProfile()
    {
        CreateMap<CreateCampaignDto, Campaign>();

        CreateMap<UpdateCampaignDto, Campaign>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Campaign, CampaignDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => MapStatus(s.Status)));

        CreateMap<Campaign, CampaignListItemDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => MapStatus(s.Status)));

        CreateMap<Campaign, CampaignReportDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => MapStatus(s.Status)));

        CreateMap<Campaign, SendEmailCommand>()
            .ForMember(d => d.CampaignId, o => o.MapFrom(s => s.Id));

    }

    private static CampaignStatusDto MapStatus(CampaignStatus status) => status switch
    {
        CampaignStatus.Draft => CampaignStatusDto.Draft,
        CampaignStatus.Scheduled => CampaignStatusDto.Scheduled,
        CampaignStatus.Sent => CampaignStatusDto.Sent,
        CampaignStatus.Failed => CampaignStatusDto.Failed,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Bilinmeyen Kampanya Durumu !")
    };
}
