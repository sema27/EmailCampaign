namespace EmailCampaign.Application.Campaigns.Dtos.Requests;

public sealed record CreateCampaignDto
{
    public required string Name { get; init; }
    public required string Subject { get; init; }
    public required string Content { get; init; }
}
//isimlendirme bak