using EmailCampaign.Application.Campaigns.Messaging;
using EmailCampaign.Application.Campaigns.Services;
using MassTransit;

namespace EmailCampaign.Worker.Consumers;

public sealed class SendEmailCommandConsumer : IConsumer<SendEmailCommand>
{
    private readonly ICampaignSendService _sendService;
    private readonly ILogger<SendEmailCommandConsumer> _log;

    public SendEmailCommandConsumer(ICampaignSendService sendService, ILogger<SendEmailCommandConsumer> log)
    {
        _sendService = sendService;
        _log = log;
    }

    public async Task Consume(ConsumeContext<SendEmailCommand> ctx)
    {
        var msg = ctx.Message;
        _log.LogInformation("Consuming SendEmailCommand for CampaignId={CampaignId}", msg.CampaignId);

        try
        {
            _log.LogInformation("Simulating send: subject='{Subject}'", msg.Subject);
            await Task.Delay(200, ctx.CancellationToken);

            var updated = await _sendService.MarkAsSentAsync(msg.CampaignId, ctx.CancellationToken);
            if (updated)
                _log.LogInformation("Campaign marked as Sent: {CampaignId}", msg.CampaignId);
            else
                _log.LogWarning("Campaign not found or already sent: {CampaignId}", msg.CampaignId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to process campaign {CampaignId}", msg.CampaignId);
            await _sendService.MarkAsFailedAsync(msg.CampaignId, ctx.CancellationToken);
        }
    }
}
