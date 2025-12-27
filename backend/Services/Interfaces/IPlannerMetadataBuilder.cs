namespace Services.Interfaces;

using Domain.Models;

public interface IPlannerMetadataBuilder {
    IReadOnlyList<PlannerFieldMetadata> Build();
}