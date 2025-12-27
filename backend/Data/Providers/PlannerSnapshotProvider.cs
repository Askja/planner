namespace Data.Providers;

using Entities;
using Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Models;
using Snapshots;

public sealed class PlannerSnapshotProvider(IMemoryCache memoryCache, IServiceScopeFactory scopeFactory, IOptions<PlannerSnapshotOptions> options) : IPlannerSnapshotProvider {
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly PlannerSnapshotOptions _options = options.Value;

    public async Task<PlannerSourceSnapshot> GetAsync(CancellationToken ct = default) {
        if (memoryCache.TryGetValue(_options.CacheKey, out PlannerSourceSnapshot? snapshot) && snapshot is not null) {
            return snapshot;
        }

        await _gate.WaitAsync(ct).ConfigureAwait(false);

        try {
            if (memoryCache.TryGetValue(_options.CacheKey, out snapshot) && snapshot is not null) {
                return snapshot;
            }

            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            IServiceProvider sp = scope.ServiceProvider;

            ISkuRepository skuRepository = sp.GetRequiredService<ISkuRepository>();
            ISkuSubRepository skuSubRepository = sp.GetRequiredService<ISkuSubRepository>();
            IHistoryY0Repository historyY0Repository = sp.GetRequiredService<IHistoryY0Repository>();
            IPlanningY1Repository planningY1Repository = sp.GetRequiredService<IPlanningY1Repository>();

            Task<IReadOnlyList<SkuEntity>> skusTask = skuRepository.GetAllAsync(true, ct);
            Task<IReadOnlyList<SkuSubEntity>> skuSubsTask = skuSubRepository.GetAllAsync(true, ct);
            Task<IReadOnlyList<HistoryY0Entity>> historyTask = historyY0Repository.GetAllAsync(true, ct);
            Task<IReadOnlyList<PlanningY1Entity>> planningTask = planningY1Repository.GetAllAsync(true, ct);

            await Task.WhenAll(skusTask, skuSubsTask, historyTask, planningTask).ConfigureAwait(false);

            IReadOnlyList<SkuEntity> skus = await skusTask.ConfigureAwait(false);
            IReadOnlyList<SkuSubEntity> skuSubs = await skuSubsTask.ConfigureAwait(false);
            IReadOnlyList<HistoryY0Entity> history = await historyTask.ConfigureAwait(false);
            IReadOnlyList<PlanningY1Entity> planning = await planningTask.ConfigureAwait(false);

            Dictionary<int, SkuEntity> skuById = skus.ToDictionary(x => x.SkuId);
            Dictionary<int, SkuSubEntity> skuSubById = skuSubs.ToDictionary(x => x.SkuSubId);

            Dictionary<int, IReadOnlyList<SkuSubEntity>> skuSubsBySkuId = skuSubs.GroupBy(x => x.SkuId).ToDictionary(g => g.Key, g => (IReadOnlyList<SkuSubEntity>)g.OrderBy(x => x.SkuSubId).ToList());

            Dictionary<string, SkuSubEntity> skuSubByName = skuSubs.GroupBy(x => x.SkuSubName, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            Dictionary<int, HistoryY0Entity> historyBySkuSubId = history.ToDictionary(x => x.SkuSubId);
            Dictionary<int, PlanningY1Entity> planningBySkuSubId = planning.ToDictionary(x => x.SkuSubId);

            snapshot = new(skus, skuSubs, history, planning, skuById, skuSubById, skuSubsBySkuId, skuSubByName, historyBySkuSubId, planningBySkuSubId);

            memoryCache.Set(
                _options.CacheKey, snapshot, new MemoryCacheEntryOptions {
                    SlidingExpiration = TimeSpan.FromSeconds(Math.Max(5, _options.SlidingExpirationSeconds)),
                    Priority = CacheItemPriority.High
                }
            );

            return snapshot;
        } finally {
            _gate.Release();
        }
    }

    public void Invalidate() {
        memoryCache.Remove(_options.CacheKey);
    }
}