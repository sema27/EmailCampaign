using EmailCampaign.Application.Stats.Dtos;

namespace EmailCampaign.Application.Stats.Services;

public interface IStatsService
{
    Task<StatsDto> GetAsync();
}
