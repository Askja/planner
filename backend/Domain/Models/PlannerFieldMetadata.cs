namespace Domain.Models;

public sealed class PlannerFieldMetadata {
    public required string Key { get; init; }
    public required string Title { get; init; }
    public required string DataType { get; init; }
    public string? Style { get; init; }
    public bool IsEditable { get; init; }
}