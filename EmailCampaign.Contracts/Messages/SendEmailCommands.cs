namespace EmailCampaign.Contracts.Messages;

public class SendEmailCommand
{
    public Guid CampaignId { get; set; }

    public string Subject { get; set; }

    public string Content { get; set; }

}
