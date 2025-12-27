namespace Logger.Middleware;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Models;
using Serilog.Context;

public sealed class CorrelationIdMiddleware(IOptions<LoggingOptions> options) : IMiddleware {
    private readonly LoggingOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
        string headerName = _options.Correlation.HeaderName;

        string correlationId = context.Request.Headers.TryGetValue(headerName, out StringValues headerValue) && !string.IsNullOrWhiteSpace(headerValue) ? headerValue.ToString() : context.TraceIdentifier;

        context.Response.Headers[headerName] = correlationId;

        using (LogContext.PushProperty("correlationId", correlationId)) {
            using (LogContext.PushProperty("traceId", context.TraceIdentifier)) {
                using (LogContext.PushProperty("requestId", context.TraceIdentifier)) {
                    await next(context).ConfigureAwait(false);
                }
            }
        }
    }
}