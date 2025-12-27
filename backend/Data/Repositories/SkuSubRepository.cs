namespace Data.Repositories;

using System.Linq.Expressions;
using Abstractions;
using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed class SkuSubRepository(DataContext context) : BaseRepository<SkuSubEntity>(context), ISkuSubRepository {
    public async Task<IReadOnlyList<SkuSubEntity>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default) {
        return await BuildQuery(null, asNoTracking).OrderBy(x => x.SkuId).ThenBy(x => x.SkuSubId).ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SkuSubEntity>> GetByNamesAsync(IReadOnlyCollection<string> skuSubNames, bool asNoTracking = true, CancellationToken ct = default) {
        if (skuSubNames.Count == 0) {
            return [];
        }

        Expression<Func<SkuSubEntity, bool>> filter = x => skuSubNames.Contains(x.SkuSubName);

        return await BuildQuery(filter, asNoTracking).OrderBy(x => x.SkuId).ThenBy(x => x.SkuSubId).ToListAsync(ct).ConfigureAwait(false);
    }
}