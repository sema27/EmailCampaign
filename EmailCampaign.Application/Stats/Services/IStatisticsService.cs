using EmailCampaign.Application.Stats.Dtos;

namespace EmailCampaign.Application.Stats.Services;

public interface IStatisticsService
{
    Task<StatisticsSummaryDto> GetAsync();
}
