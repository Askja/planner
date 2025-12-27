namespace Data.Interfaces;

using Entities;

public interface ISkuSubRepository : IBaseRepository<SkuSubEntity> {
    Task<IReadOnlyList<SkuSubEntity>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default);

    Task<IReadOnlyList<SkuSubEntity>> GetByNamesAsync(IReadOnlyCollection<string> skuSubNames, bool asNoTracking = true, CancellationToken ct = default);
}