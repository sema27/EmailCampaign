namespace EmailCampaign.Application.Campaigns.Dtos.Requests;

public sealed record ScheduleCampaignDto
{
    public DateTime ScheduledAtUtc { get; init; }
}
