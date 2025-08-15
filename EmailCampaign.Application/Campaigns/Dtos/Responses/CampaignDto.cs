namespace EmailCampaign.Application.Campaigns.Dtos.Responses;

public sealed record CampaignDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Subject { get; init; }
    public required string Content { get; init; }
    public required CampaignStatusDto Status { get; init; }
    public DateTime? SentAt { get; init; }
    public required int TotalEmailsSent { get; init; }
}