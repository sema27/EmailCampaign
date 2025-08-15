namespace EmailCampaign.Application.Stats.Dtos;

public sealed record StatsDto(
    int TotalCampaigns,
    int Draft,
    int Scheduled,
    int Sent,
    int Failed
);
