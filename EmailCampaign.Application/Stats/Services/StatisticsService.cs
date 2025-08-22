using AutoMapper;
using EmailCampaign.Application.Common.Repositories;
using EmailCampaign.Application.Stats.Dtos;
using EmailCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmailCampaign.Application.Stats.Services;

public sealed class StatisticsService : IStatisticsService
{
    private readonly IGenericRepository<Campaign, Guid> _repository;

    public StatisticsService(IGenericRepository<Campaign, Guid> repository, IMapper _)
    {
        _repository = repository;
    }

    public async Task<StatisticsSummaryDto> GetAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var agg = await _repository.Query()   
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Draft = g.Count(c => c.Status == CampaignStatus.Draft),
                Scheduled = g.Count(c => c.Status == CampaignStatus.Scheduled),
                Sent = g.Count(c => c.Status == CampaignStatus.Sent),
                Failed = g.Count(c => c.Status == CampaignStatus.Failed),
                TotalEmailsSent = g.Sum(c => (int?)c.TotalEmailsSent) ?? 0,
                SentToday = g.Count(c => c.SentAt.HasValue &&
                                               c.SentAt.Value >= today &&
                                               c.SentAt.Value < tomorrow)
            })
            .FirstOrDefaultAsync();

        if (agg is null)
            return new StatisticsSummaryDto(0, 0, 0, 0, 0, 0, 0);

        return new StatisticsSummaryDto(
            TotalCampaigns: agg.Total,
            Draft: agg.Draft,
            Scheduled: agg.Scheduled,
            Sent: agg.Sent,
            Failed: agg.Failed,
            TotalEmailsSent: agg.TotalEmailsSent,
            SentToday: agg.SentToday
        );
    }

}
