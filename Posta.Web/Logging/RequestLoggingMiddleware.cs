using Microsoft.AspNetCore.Http.Extensions;
using System.Diagnostics;
namespace Posta.Web.Logging;
public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context) {
        var started = DateTimeOffset.Now;
        var timer = Stopwatch.StartNew();
        var url = context.Request.GetDisplayUrl();
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        logger.LogInformation("URL: {Url} | Time: {Time:O} | IP: {Ip} | Method: {Method}", url, started, ip, context.Request.Method);
        try { await next(context); }
        finally { logger.LogInformation("Completed: {TraceId} | Status: {Status} | DurationMs: {Duration}", context.TraceIdentifier, context.Response.StatusCode, timer.ElapsedMilliseconds); }
    }
}
