namespace EmailCampaign.Application.Campaigns.Dtos.Responses;

public sealed record DeliverySummaryDto
{
    public required int TotalCampaigns { get; init; }
    public required int TotalEmailsSent { get; init; }
    public required int ActiveCampaigns { get; init; }
    public required int SentCampaigns { get; init; }
    public DateTime GeneratedAtUtc { get; init; } = DateTime.UtcNow;
}
