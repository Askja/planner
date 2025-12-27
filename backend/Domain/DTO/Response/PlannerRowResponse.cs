namespace Domain.DTO.Response;

using Models;

public sealed class PlannerRowResponse {
    public required IReadOnlyList<PlannerRow> Data { get; init; }
    public required IReadOnlyList<PlannerFieldMetadata> Metadata { get; init; }
}