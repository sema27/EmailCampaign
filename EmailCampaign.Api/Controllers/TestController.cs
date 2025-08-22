using EmailCampaign.Application.Common.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly AnyService _svc;
    public TestController(AnyService svc) => _svc = svc;

    [HttpGet("external")]
    public async Task<IActionResult> External()
    {
        var (ok, content, status) = await _svc.TryCallAsync();
        if (!ok)
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "Upstream error", upstreamStatus = status });

        return Ok(content);
    }
}
