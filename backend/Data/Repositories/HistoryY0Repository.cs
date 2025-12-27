namespace Data.Repositories;

using Abstractions;
using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed class HistoryY0Repository(DataContext context) : BaseRepository<HistoryY0Entity>(context), IHistoryY0Repository {
    public async Task<IReadOnlyList<HistoryY0Entity>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default) {
        return await BuildQuery(null, asNoTracking).OrderBy(x => x.SkuSubId).ToListAsync(ct).ConfigureAwait(false);
    }
}