using EmailCampaign.Application.Stats.Dtos;
using EmailCampaign.Application.Stats.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EmailCampaign.Api.Controllers;

[ApiController]
[Route("api/v1/statistics")]
[Produces("application/json")]
[SwaggerTag("Genel istatistikler")]
public sealed class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statistics;

    public StatisticsController(IStatisticsService statistics) => _statistics = statistics;

    [HttpGet]
    [SwaggerOperation(
        OperationId = "Statistics_GetSummary",
        Summary = "Genel istatistikler",
        Description = "Toplam kampanya sayısı, bekleyen (Scheduled), gönderilen, hata alan ve toplam gönderilen e-posta sayısı ile bugün gönderilen kampanya sayısını döner.")]
    [ProducesResponseType(typeof(StatisticsSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StatisticsSummaryDto>> GetSummary()
    {
        var dto = await _statistics.GetAsync();
        return Ok(dto);
    }
}
