using System.Net;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EmailCampaign.Api.Extensions;

public static class ExceptionHandlingExtensions
{
    public static void UseGlobalExceptionHandling(this IApplicationBuilder app, ILogger logger)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                var ex = feature?.Error;

                ProblemDetails pd;

                switch (ex)
                {
                    case ValidationException fvEx:
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        pd = new ProblemDetails
                        {
                            Title = "Validation error",
                            Status = StatusCodes.Status400BadRequest,
                            Type = "https://tools.ietf.org/html/rfc7807",
                            Detail = "Request validation failed."
                        };
                        // FluentValidation hata detaylarını ek bilgi olarak koy
                        pd.Extensions["errors"] = fvEx.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                        break;

                    case KeyNotFoundException:
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        pd = new ProblemDetails
                        {
                            Title = "Resource not found",
                            Status = StatusCodes.Status404NotFound
                        };
                        break;

                    case UnauthorizedAccessException:
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        pd = new ProblemDetails
                        {
                            Title = "Unauthorized",
                            Status = StatusCodes.Status401Unauthorized
                        };
                        break;

                    default:
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        pd = new ProblemDetails
                        {
                            Title = "Server error",
                            Status = StatusCodes.Status500InternalServerError,
                            Detail = "An unexpected error occurred."
                        };
                        break;
                }

                pd.Extensions["traceId"] = context.TraceIdentifier;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(pd);
            });
        });
    }
}
