namespace Domain.DTO.Request;

public sealed class PlannerRowRequest {
    public required int SkuSubId { get; init; }
    public required decimal PlanningUnits { get; init; }
}