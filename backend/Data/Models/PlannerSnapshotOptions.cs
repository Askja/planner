namespace Data.Models;

public class PlannerSnapshotOptions {
    public string CacheKey { get; set; } = "planner:snapshot:v1";
    public int SlidingExpirationSeconds { get; set; } = 60;
}