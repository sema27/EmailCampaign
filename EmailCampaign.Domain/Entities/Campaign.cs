namespace EmailCampaign.Domain.Entities
{
    public class Campaign : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Subject { get; set; } = default!;
        public string Content { get; set; } = default!;

        public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

        public DateTime? ScheduledAt { get; set; }

        public DateTime? SentAt { get; set; }

        public int TotalEmailsSent { get; set; } = 0;
    }
}
