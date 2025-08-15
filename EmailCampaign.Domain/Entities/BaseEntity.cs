namespace EmailCampaign.Domain.Entities;

    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public void SetUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
