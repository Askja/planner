namespace Logger.Middleware;

using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;

public sealed class SlowRequestLoggingMiddleware(ILogger<SlowRequestLoggingMiddleware> logger, IOptions<LoggingOptions> options) : IMiddleware {
    private readonly LoggingOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
        Stopwatch sw = Stopwatch.StartNew();

        try {
            await next(context).ConfigureAwait(false);
        } finally {
            sw.Stop();

            int thresholdMs = _options.RequestLogging.SlowRequestThresholdMs;

            if (thresholdMs > 0 && sw.ElapsedMilliseconds >= thresholdMs) {
                logger.LogWarning("Медленный запрос: {method} {path} ответ {statusCode} за {elapsedMs}мс", context.Request.Method, context.Request.Path.Value, context.Response.StatusCode, sw.ElapsedMilliseconds);
            }
        }
    }
}