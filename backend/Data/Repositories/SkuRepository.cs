namespace Data.Repositories;

using Abstractions;
using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed class SkuRepository(DataContext context) : BaseRepository<SkuEntity>(context), ISkuRepository {
    public async Task<IReadOnlyList<SkuEntity>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default) {
        return await BuildQuery(null, asNoTracking).OrderBy(x => x.SkuId).ToListAsync(ct).ConfigureAwait(false);
    }
}