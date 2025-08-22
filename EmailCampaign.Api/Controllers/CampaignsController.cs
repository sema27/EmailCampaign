using EmailCampaign.Application.Campaigns.Commands.StartSendCampaign;
using EmailCampaign.Application.Campaigns.Dtos.Requests;
using EmailCampaign.Application.Campaigns.Dtos.Responses;
using EmailCampaign.Application.Campaigns.Enums;
using EmailCampaign.Application.Campaigns.Services;
using EmailCampaign.Application.Common.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace EmailCampaign.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[SwaggerTag("Kampanya CRUD, kuyruğa gönderim, rapor uçları")]
public sealed class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaignService;
    private readonly ICommandHandler<StartSendCampaignCommand, EnqueueResult> _startSendHandler;

    public CampaignsController(
        ICampaignService campaignService,
        ICommandHandler<StartSendCampaignCommand, EnqueueResult> startSendHandler)
    {
        _campaignService = campaignService;
        _startSendHandler = startSendHandler;
    }

    // POST api/v1/campaigns
    [HttpPost]
    [SwaggerOperation(
        OperationId = "Campaigns_Create",
        Summary = "Kampanya oluştur",
        Description = "Ad, konu ve içerik ile yeni kampanya oluşturur.")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CampaignDto>> Create(
        [FromBody] CreateCampaignDto dto)
    {
        var created = await _campaignService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // GET api/v1/campaigns/{id}
    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [SwaggerOperation(
        OperationId = "Campaigns_GetById",
        Summary = "Kampanya detayı",
        Description = "Kimliğe göre kampanya bilgisi getirir.")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CampaignDto>> GetById(
        [FromRoute] Guid id)
    {
        var campaign = await _campaignService.GetByIdAsync(id);
        return campaign is null ? NotFound() : Ok(campaign);
    }

    // GET api/v1/campaigns?page=1&pageSize=50
    [HttpGet]
    [SwaggerOperation(
        OperationId = "Campaigns_GetAll",
        Summary = "Kampanya listesi",
        Description = "Sayfa ve sayfa boyuna göre liste.")]
    [ProducesResponseType(typeof(IReadOnlyList<CampaignListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CampaignListItemDto>>> GetAll(
        [FromQuery, Range(1, int.MaxValue, ErrorMessage = "page en az 1 olmalıdır.")]
        int page = 1,
        [FromQuery, Range(1, 200, ErrorMessage = "pageSize 1-200 arasında olmalıdır.")]
        int pageSize = 50)
    {
        // [ApiController] attribute'u ile model state invalid ise otomatik 400 döner.
        var campaigns = await _campaignService.GetAllAsync(page, pageSize);

        Response.Headers["X-Page"] = page.ToString();
        Response.Headers["X-Page-Size"] = pageSize.ToString();

        return Ok(campaigns);
    }

    // PUT api/v1/campaigns/{id}
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        OperationId = "Campaigns_Update",
        Summary = "Kampanyayı güncelle",
        Description = "Ad, konu veya içerikte değişiklik yapar. (Gönderilmeyen alanlar korunur)")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CampaignDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateCampaignDto dto)
    {
        // UpdateAsync muhtemelen Task<CampaignDto?> döndürüyor
        var updated = await _campaignService.UpdateAsync(id, dto);
        if (updated is null) return NotFound();

        return Ok(updated);
    }


    // DELETE api/v1/campaigns/{id}
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        OperationId = "Campaigns_Delete",
        Summary = "Kampanyayı sil",
        Description = "Belirtilen kampanyayı kalıcı olarak siler.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id)
    {
        var deleted = await _campaignService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    // POST api/v1/campaigns/{id}/send
    [HttpPost("{id:guid}/send")]
    [SwaggerOperation(
        OperationId = "Campaigns_Send",
        Summary = "Gönderimi başlat",
        Description = "Kampanyayı RabbitMQ kuyruğuna yazar (enqueue). İdempotent davranır.")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Send(
        [FromRoute] Guid id)
    {
        var result = await _startSendHandler.Handle(new StartSendCampaignCommand(id));

        return result switch
        {
            EnqueueResult.NotFound => NotFound(),
            EnqueueResult.AlreadySent => NoContent(),
            EnqueueResult.Enqueued => Accepted(),
            EnqueueResult.Failed => Problem(
                                            title: "Kuyruğa yazma başarısız",
                                            statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => Problem(
                                            title: "Bilinmeyen durum",
                                            statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    // GET api/v1/campaigns/{id}/report
    [HttpGet("{id:guid}/report")]
    [SwaggerOperation(
        OperationId = "Campaigns_GetReport",
        Summary = "Kampanya raporu",
        Description = "Toplam gönderilen e-posta sayısı ve kampanya durumu.")]
    [ProducesResponseType(typeof(CampaignReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CampaignReportDto>> GetReport(
        [FromRoute] Guid id)
    {
        var report = await _campaignService.GetReportAsync(id);
        return report is null ? NotFound() : Ok(report);
    }
}
