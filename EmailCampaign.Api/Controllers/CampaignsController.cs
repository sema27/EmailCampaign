using EmailCampaign.Application.Campaigns.Commands.StartSendCampaign;
using EmailCampaign.Application.Campaigns.Dtos.Requests;
using EmailCampaign.Application.Campaigns.Dtos.Responses;
using EmailCampaign.Application.Campaigns.Enums;
using EmailCampaign.Application.Campaigns.Services;
using EmailCampaign.Application.Common.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

    [HttpPost]
    [SwaggerOperation(
        OperationId = "Campaign_Create",
        Summary = "Kampanya oluştur",
        Description = "Ad, konu ve içerik ile yeni kampanya oluşturur.")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CampaignDto>> Create([FromBody] CreateCampaignDto dto)
    {
        var created = await _campaignService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}", Name = nameof(GetById))]
    [SwaggerOperation(
        OperationId = "Campaign_GetById",
        Summary = "Kampanya detayı",
        Description = "Kimliğe göre kampanya bilgisi getirir.")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CampaignDto>> GetById(Guid id)
    {
        var campaign = await _campaignService.GetByIdAsync(id);
        return campaign is null ? NotFound() : Ok(campaign);
    }

    [HttpGet]
    [SwaggerOperation(
        OperationId = "Campaign_GetAll",
        Summary = "Kampanya listesi",
        Description = "Sayfa ve sayfa boyuna göre liste.")]
    [ProducesResponseType(typeof(IReadOnlyList<CampaignListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CampaignListItemDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (page < 1) return ValidationProblem("page en az 1 olmalıdır.");
        pageSize = Math.Clamp(pageSize, 1, 200);

        var campaigns = await _campaignService.GetAllAsync(page, pageSize);

        Response.Headers["X-Page"] = page.ToString();
        Response.Headers["X-Page-Size"] = pageSize.ToString();

        return Ok(campaigns);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        OperationId = "Campaign_Update",
        Summary = "Kampanyayı güncelle",
        Description = "Ad, konu veya içerikte değişiklik yapar. (Gönderilmeyen alanlar korunur)")]
    [ProducesResponseType(typeof(CampaignDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignDto dto)
    {
        var ok = await _campaignService.UpdateAsync(id, dto);
        if (!ok) return NotFound();

        var fresh = await _campaignService.GetByIdAsync(id);
        return Ok(fresh); 
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        OperationId = "Campaign_Delete",
        Summary = "Kampanyayı sil",
        Description = "Belirtilen kampanyayı kalıcı olarak siler.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _campaignService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/send")]
    [SwaggerOperation(
        OperationId = "Campaign_Send",
        Summary = "Gönderimi başlat",
        Description = "Kampanyayı RabbitMQ kuyruğuna yazar (enqueue). İdempotent davranır.")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status204NoContent)] // zaten Sent ise
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Send(Guid id)
    {
        var result = await _startSendHandler.Handle(new StartSendCampaignCommand(id));

        return result switch
        {
            EnqueueResult.NotFound => NotFound(),
            EnqueueResult.AlreadySent => NoContent(),
            EnqueueResult.Enqueued => Accepted(),
            EnqueueResult.Failed => Problem(title: "Enqueue başarısız", statusCode: StatusCodes.Status400BadRequest),
            _ => Problem(title: "Bilinmeyen durum", statusCode: StatusCodes.Status400BadRequest)
        };
    }

    [HttpGet("{id:guid}/report")]
    [SwaggerOperation(
        OperationId = "Campaign_GetReport",
        Summary = "Kampanya raporu",
        Description = "Toplam gönderilen e‑posta sayısı ve kampanya durumu.")]
    [ProducesResponseType(typeof(CampaignReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CampaignReportDto>> GetReport(Guid id)
    {
        var report = await _campaignService.GetReportAsync(id);
        return report is null ? NotFound() : Ok(report);
    }
}
