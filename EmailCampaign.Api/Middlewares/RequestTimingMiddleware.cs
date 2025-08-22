using System.Diagnostics;

namespace EmailCampaign.Api.Middlewares;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();
        _logger.LogInformation("Handled {Method} {Path} in {Elapsed} ms (status {Status})",
            context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds, context.Response.StatusCode);
    }
}
