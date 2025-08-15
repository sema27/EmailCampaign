namespace EmailCampaign.Application.Campaigns.Dtos.Requests;

public sealed class UpdateCampaignDto
{
    public string? Name { get; set; }
    public string? Subject { get; set; }
    public string? Content { get; set; }
}
