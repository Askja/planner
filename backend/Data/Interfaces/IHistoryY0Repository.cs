namespace Data.Interfaces;

using Entities;

public interface IHistoryY0Repository : IBaseRepository<HistoryY0Entity> {
    Task<IReadOnlyList<HistoryY0Entity>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default);
}