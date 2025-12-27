namespace Domain.Models;

using Enums;

public sealed class PlannerOptions {
    public bool EditablePlanningUnitsOnly { get; set; } = true;
    public PlannerLevel[] DefaultLevels { get; set; } = [PlannerLevel.Total, PlannerLevel.Sku, PlannerLevel.SkuSub,];
}