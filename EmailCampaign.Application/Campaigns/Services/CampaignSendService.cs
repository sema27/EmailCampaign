using EmailCampaign.Domain.Entities;
using EmailCampaign.Application.Common.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmailCampaign.Application.Campaigns.Services;

public sealed class CampaignSendService : ICampaignSendService
{
    private readonly IGenericRepository<Campaign, Guid> _repository;

    public CampaignSendService(IGenericRepository<Campaign, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<bool> MarkAsSentAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _repository.Query()
            .FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken);

        if (campaign is null) return false;

        if (campaign.Status != CampaignStatus.Sent)
        {
            campaign.Status = CampaignStatus.Sent;
            campaign.SentAt = DateTime.UtcNow;
            campaign.TotalEmailsSent++;
            await _repository.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> MarkAsFailedAsync(Guid campaignId, CancellationToken cancellationToken = default)
    {
        var campaign = await _repository.Query()
            .FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken);

        if (campaign is null) return false;

        campaign.Status = CampaignStatus.Failed;
        await _repository.SaveChangesAsync();
        return true;
    }
}
