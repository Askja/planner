namespace Logger.Extensions;

using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware;
using Models;
using Serilog;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

public static class LoggingExtensions {
    private const string SerilogConfigFileName = "serilog.config.json";

    public static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder) {
        string baseDir = AppContext.BaseDirectory;
        string serilogConfigPath = Path.Combine(baseDir, SerilogConfigFileName);

        if (File.Exists(serilogConfigPath)) {
            builder.Configuration.AddJsonFile(serilogConfigPath, false, true);
        }

        builder.Services.Configure<LoggingOptions>(builder.Configuration.GetSection("Logging"));

        builder.Host.UseSerilog(
            (context, services, cfg) => {
                cfg.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services).Enrich.FromLogContext().Enrich.WithProperty("application", context.HostingEnvironment.ApplicationName).Enrich.WithProperty("environment", context.HostingEnvironment.EnvironmentName).Enrich.WithProperty("assembly", Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown");

                if (!context.Configuration.GetSection("Serilog").Exists()) {
                    cfg.MinimumLevel.Information().MinimumLevel.Override("Microsoft", LogEventLevel.Warning).WriteTo.Console();
                }
            }
        );

        builder.Services.AddTransient<CorrelationIdMiddleware>();
        builder.Services.AddTransient<SlowRequestLoggingMiddleware>();

        return builder;
    }

    public static WebApplication UseLogging(this WebApplication app) {
        LoggingOptions options = app.Services.GetRequiredService<IOptions<LoggingOptions>>().Value;

        app.UseMiddleware<CorrelationIdMiddleware>();

        if (options.RequestLogging.Enable) {
            app.UseSerilogRequestLogging(
                o => {
                    o.MessageTemplate = options.RequestLogging.MessageTemplate;

                    o.EnrichDiagnosticContext = (ctx, http) => {
                        ctx.Set("requestHost", http.Request.Host.Value);
                        ctx.Set("requestScheme", http.Request.Scheme);
                        ctx.Set("remoteIp", http.Connection.RemoteIpAddress?.ToString());
                        ctx.Set("userAgent", http.Request.Headers.UserAgent.ToString());
                        ctx.Set("queryString", http.Request.QueryString.Value);
                        ctx.Set("contentType", http.Request.ContentType);
                        ctx.Set("contentLength", http.Request.ContentLength);
                    };
                }
            );

            app.UseMiddleware<SlowRequestLoggingMiddleware>();
        }

        return app;
    }

    public static WebApplication UseExceptionHandling(this WebApplication app) {
        app.UseExceptionHandler(
            errorApp => {
                errorApp.Run(
                    async context => {
                        IExceptionHandlerFeature? feature = context.Features.Get<IExceptionHandlerFeature>();
                        Exception? ex = feature?.Error;

                        LoggingOptions options = context.RequestServices.GetRequiredService<IOptions<LoggingOptions>>().Value;

                        ILogger logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");

                        if (ex is not null) {
                            logger.LogError(ex, "Unhandled exception for {method} {path}", context.Request.Method, context.Request.Path.Value);
                        }

                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                        Dictionary<string, object?> payload = new() {
                            ["title"] = "Внутренняя ошибка сервера",
                            ["status"] = 500,
                            ["traceId"] = context.TraceIdentifier
                        };

                        if (options.ExceptionHandling.IncludeExceptionDetails && ex is not null) {
                            payload["exception"] = ex.GetType().FullName;
                            payload["message"] = ex.Message;
                        }

                        await context.Response.WriteAsJsonAsync(payload).ConfigureAwait(false);
                    }
                );
            }
        );

        return app;
    }
}