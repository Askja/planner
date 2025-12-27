namespace Logger.Models;

public sealed class CorrelationOptions {
    public string HeaderName { get; set; } = "X-Correlation-ID";
}