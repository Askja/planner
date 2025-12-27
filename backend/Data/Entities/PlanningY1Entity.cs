namespace Data.Entities;

using Abstractions;

public sealed class PlanningY1Entity : BaseEntity {
    public int SkuSubId { get; set; }

    public decimal Units { get; set; }

    public decimal Amount { get; set; }

    public SkuSubEntity? SkuSub { get; set; }
}