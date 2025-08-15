using AutoMapper;
using EmailCampaign.Application.Common.Repositories;
using EmailCampaign.Application.Stats.Dtos;
using EmailCampaign.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmailCampaign.Application.Stats.Services;

public sealed class StatsService : IStatsService
{
    private readonly IGenericRepository<Campaign, Guid> _repository;

    public StatsService(IGenericRepository<Campaign, Guid> repository, IMapper _)
    {
        _repository = repository;
    }

    public async Task<StatsDto> GetAsync()
    {
        var today = DateTime.UtcNow.Date;

        // Tek round-trip ile tüm sayıları hesaplayalım
        var agg = await _repository.Query()
            .AsNoTracking()
            .GroupBy(_ => 1) // hepsini tek grupta topla
            .Select(g => new
            {
                Total = g.Count(),
                Draft = g.Count(c => c.Status == CampaignStatus.Draft),
                Scheduled = g.Count(c => c.Status == CampaignStatus.Scheduled),
                Sent = g.Count(c => c.Status == CampaignStatus.Sent),
                Failed = g.Count(c => c.Status == CampaignStatus.Failed),
                TotalEmailsSent = g.Sum(c => c.TotalEmailsSent),
                SentToday = g.Count(c => c.SentAt.HasValue && c.SentAt.Value.Date == today)
            })
            .FirstOrDefaultAsync();

        // Hiç kayıt yoksa sıfırlarla dön
        if (agg is null)
            return new StatsDto(0, 0, 0, 0, 0, 0, 0);

        return new StatsDto(
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
