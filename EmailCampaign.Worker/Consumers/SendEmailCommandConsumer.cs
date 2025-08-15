using EmailCampaign.Application.Campaigns.Messaging;
using EmailCampaign.Domain.Entities;
using EmailCampaign.Infrastructure.Persistance;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EmailCampaign.Worker.Consumers;

public sealed class SendEmailCommandConsumer : IConsumer<SendEmailCommand>
{
    private readonly AppDbContext _db;
    private readonly ILogger<SendEmailCommandConsumer> _log;

    public SendEmailCommandConsumer(AppDbContext db, ILogger<SendEmailCommandConsumer> log)
    {
        _db = db;
        _log = log;
    }

    public async Task Consume(ConsumeContext<SendEmailCommand> ctx)
    {
        var msg = ctx.Message;
        _log.LogInformation("Consuming SendEmailCommand for CampaignId={CampaignId}", msg.CampaignId);

        var campaign = await _db.Campaigns.FirstOrDefaultAsync(c => c.Id == msg.CampaignId, ctx.CancellationToken);
        if (campaign is null)
        {
            _log.LogWarning("Campaign not found: {CampaignId}", msg.CampaignId);
            return; 
        }

        try
        {
            _log.LogInformation("Simulating send: subject='{Subject}'", msg.Subject);
            await Task.Delay(200, ctx.CancellationToken);

            if (campaign.Status != CampaignStatus.Sent)
            {
                campaign.Status = CampaignStatus.Sent;
                campaign.SentAt = DateTime.UtcNow;
                campaign.TotalEmailsSent++;   
                await _db.SaveChangesAsync(ctx.CancellationToken);
                _log.LogInformation("Campaign marked as Sent: {CampaignId}", msg.CampaignId);
            }
            else
            {
                _log.LogInformation("Campaign already Sent, skipping increment: {CampaignId}", msg.CampaignId);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to process campaign {CampaignId}", msg.CampaignId);
            campaign.Status = CampaignStatus.Failed;
            await _db.SaveChangesAsync(ctx.CancellationToken);
        }
    }
}
