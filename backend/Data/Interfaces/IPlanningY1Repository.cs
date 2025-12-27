namespace Data.Interfaces;

using Entities;

public interface IPlanningY1Repository : IBaseRepository<PlanningY1Entity> {
    Task<IReadOnlyList<PlanningY1Entity>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default);
    Task UpsertAsync(int skuSubId, decimal units, decimal amount, CancellationToken ct = default);
}