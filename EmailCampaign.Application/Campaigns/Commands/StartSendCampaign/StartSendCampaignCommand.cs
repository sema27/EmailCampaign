using EmailCampaign.Application.Campaigns.Enums;
using EmailCampaign.Application.Common.Abstractions;

namespace EmailCampaign.Application.Campaigns.Commands.StartSendCampaign;

public sealed record StartSendCampaignCommand(Guid CampaignId)
    : ICommand<EnqueueResult>;
