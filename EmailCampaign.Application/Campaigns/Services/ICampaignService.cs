using EmailCampaign.Application.Campaigns.Dtos.Requests;
using EmailCampaign.Application.Campaigns.Dtos.Responses;
using EmailCampaign.Application.Campaigns.Enums;

namespace EmailCampaign.Application.Campaigns.Services;
public interface ICampaignService
{
    Task<CampaignDto> CreateAsync(CreateCampaignDto dto);
    Task<CampaignDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<CampaignListItemDto>> GetAllAsync(int page, int pageSize);
    Task<CampaignDto?> UpdateAsync(Guid id, UpdateCampaignDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<EnqueueResult> EnqueueSendAsync(Guid id);
    Task<CampaignReportDto?> GetReportAsync(Guid id);
}