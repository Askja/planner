namespace Logger.Models;

public sealed class LoggingOptions {
    public CorrelationOptions Correlation { get; set; } = new();

    public RequestLoggingOptions RequestLogging { get; set; } = new();

    public ExceptionHandlingOptions ExceptionHandling { get; set; } = new();
}