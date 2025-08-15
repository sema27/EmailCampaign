namespace EmailCampaign.Application.Campaigns.Enums;

public enum EnqueueResult
{
    NotFound = 0,
    AlreadySent = 1,
    Enqueued = 2,
    Failed = 3
}
