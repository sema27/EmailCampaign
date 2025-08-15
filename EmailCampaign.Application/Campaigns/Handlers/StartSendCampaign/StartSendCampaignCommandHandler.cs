using EmailCampaign.Application.Campaigns.Commands.StartSendCampaign;
using EmailCampaign.Application.Campaigns.Enums;
using EmailCampaign.Application.Campaigns.Services;
using EmailCampaign.Application.Common.Abstractions;

namespace EmailCampaign.Application.Campaigns.Handlers.StartSendCampaign;

public sealed class StartSendCampaignCommandHandler
    : ICommandHandler<StartSendCampaignCommand, EnqueueResult>
{
    private readonly ICampaignService _service;

    public StartSendCampaignCommandHandler(ICampaignService service)
        => _service = service;

    public async Task<EnqueueResult> Handle(StartSendCampaignCommand command, CancellationToken ct = default)
        => await _service.EnqueueSendAsync(command.CampaignId);
}
