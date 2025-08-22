namespace EmailCampaign.Application.Stats.Dtos;

public sealed record StatisticsSummaryDto(
    int TotalCampaigns,       
    int Draft,             
    int Scheduled,          
    int Sent,                 
    int Failed,               
    int TotalEmailsSent,     
    int SentToday             
);