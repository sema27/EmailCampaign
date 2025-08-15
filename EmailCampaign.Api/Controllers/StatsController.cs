using EmailCampaign.Application.Stats.Dtos;
using EmailCampaign.Application.Stats.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EmailCampaign.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Genel istatistikler")]
public sealed class StatsController : ControllerBase
{
    private readonly IStatsService _stats;

    public StatsController(IStatsService stats) => _stats = stats;

    /// <summary>Genel kampanya istatistiklerini getirir.</summary>
    [HttpGet]
    [SwaggerOperation(
        OperationId = "Stats_Get",
        Summary = "Genel istatistikler",
        Description = "Toplam kampanya sayısı, bekleyen (Scheduled), gönderilen, hata alan ve toplam gönderilen e‑posta sayısı ile bugün gönderilen kampanya sayısını döner.")]
    [ProducesResponseType(typeof(StatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StatsDto>> Get()
    {
        var dto = await _stats.GetAsync();
        return Ok(dto);
    }
}
