namespace Data.Interfaces;

using Entities;

public interface ISkuRepository : IBaseRepository<SkuEntity> {
    Task<IReadOnlyList<SkuEntity>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default);
}