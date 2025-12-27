namespace Logger.Models;

public sealed class RequestLoggingOptions {
    public bool Enable { get; set; } = true;

    public string MessageTemplate { get; set; } = "HTTP {RequestMethod} {RequestPath} ответ {StatusCode} за {Elapsed:0.0000} мс";

    public int SlowRequestThresholdMs { get; set; } = 1000;
}