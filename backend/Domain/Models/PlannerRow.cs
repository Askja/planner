namespace Domain.Models;

using Enums;

public class PlannerRow {
    public required PlannerLevel Level { get; init; }
    public required PlannerValueType ValueType { get; init; }
    public int? SkuId { get; init; }
    public string? SkuName { get; init; }
    public int? SkuSubId { get; init; }
    public string? SkuSubName { get; init; }
    public decimal HistoryY0 { get; init; }
    public decimal PlanningY1 { get; init; }
    public decimal ContributionGrowth { get; init; }
}