using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmailCampaign.Application.Campaigns.Services;

public interface ICampaignSendService
{
    Task<bool> MarkAsSentAsync(Guid campaignId, CancellationToken cancellationToken = default);
    Task<bool> MarkAsFailedAsync(Guid campaignId, CancellationToken cancellationToken = default);
}
