namespace EmailCampaign.Application.Campaigns.Messaging;

public sealed class SendEmailCommand
{
    public Guid CampaignId { get; init; }
    public string Name { get; init; } = "";
    public string Subject { get; init; } = "";
    public string Content { get; init; } = "";
}
