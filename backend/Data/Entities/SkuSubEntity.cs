namespace Data.Entities;

using Abstractions;

public sealed class SkuSubEntity : BaseEntity {
    public int SkuSubId { get; set; }

    public int SkuId { get; set; }

    public string SkuSubName { get; set; } = string.Empty;

    public decimal SkuPrice { get; set; }

    public decimal SkuRatio { get; set; }

    public SkuEntity? Sku { get; set; }

    public HistoryY0Entity? HistoryY0 { get; set; }

    public PlanningY1Entity? PlanningY1 { get; set; }
}