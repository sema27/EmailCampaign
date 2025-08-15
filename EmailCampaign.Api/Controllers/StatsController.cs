using EmailCampaign.Application.Stats.Dtos;
using EmailCampaign.Domain.Entities;
using EmailCampaign.Infrastructure.Persistance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace EmailCampaign.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Genel istatistikler")]
public sealed class StatsController : ControllerBase
{
    private readonly AppDbContext _db;
    public StatsController(AppDbContext db) => _db = db;

    ///Genel kampanya istatistikleri
    [HttpGet]
    [SwaggerOperation(Summary = "İstatistikleri getir", Description = "Toplam kampanya ve durum bazlı sayıları tek sorguda döner.")]
    [ProducesResponseType(typeof(StatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StatsDto>> Get()
    {
        // Tek round-trip: status'e göre gruplayıp say
        var groups = await _db.Campaigns
            .AsNoTracking()
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int draft = groups.FirstOrDefault(x => x.Status == CampaignStatus.Draft)?.Count ?? 0;
        int scheduled = groups.FirstOrDefault(x => x.Status == CampaignStatus.Scheduled)?.Count ?? 0;
        int sent = groups.FirstOrDefault(x => x.Status == CampaignStatus.Sent)?.Count ?? 0;
        int failed = groups.FirstOrDefault(x => x.Status == CampaignStatus.Failed)?.Count ?? 0;

        var dto = new StatsDto(
            TotalCampaigns: draft + scheduled + sent + failed,
            Draft: draft,
            Scheduled: scheduled,
            Sent: sent,
            Failed: failed
        );

        return Ok(dto);
    }
}
