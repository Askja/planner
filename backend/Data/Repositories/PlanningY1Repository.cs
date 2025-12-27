namespace Data.Repositories;

using Abstractions;
using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed class PlanningY1Repository(DataContext context) : BaseRepository<PlanningY1Entity>(context), IPlanningY1Repository {
    public async Task<IReadOnlyList<PlanningY1Entity>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default) {
        return await BuildQuery(null, asNoTracking).OrderBy(x => x.SkuSubId).ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task UpsertAsync(int skuSubId, decimal units, decimal amount, CancellationToken ct = default) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(skuSubId);
        ArgumentOutOfRangeException.ThrowIfNegative(units);
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        PlanningY1Entity? entity = await Set.FirstOrDefaultAsync(x => x.SkuSubId == skuSubId, ct).ConfigureAwait(false);

        if (entity is null) {
            entity = new() {
                SkuSubId = skuSubId,
                Units = units,
                Amount = amount
            };

            await Set.AddAsync(entity, ct).ConfigureAwait(false);
        } else {
            entity.Units = units;
            entity.Amount = amount;
            Set.Update(entity);
        }

        await Context.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}