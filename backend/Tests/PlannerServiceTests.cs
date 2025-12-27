namespace Tests;

using System.Linq.Expressions;
using Data.Entities;
using Data.Interfaces;
using Data.Snapshots;
using Domain.DTO.Response;
using Domain.Enums;
using Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Services.Services;

public sealed class PlannerServiceTests {
    [Fact]
    public async Task SkuUnits_ShouldBeWeightedSum_ByRatio() {
        PlannerSourceSnapshot snapshot = BuildSimpleSnapshot();
        FakePlannerSnapshotProvider snapshotProvider = new(snapshot);

        FakePlanningY1Repository planningRepo = new(snapshot.PlanningBySkuSubId.Values.ToList());
        FakeSkuSubRepository skuSubRepo = new(snapshot.SkuSubs);

        PlannerService service = new(snapshotProvider, planningRepo, skuSubRepo, new PlannerMetadataBuilder(Options.Create(new PlannerOptions())), Options.Create(new PlannerOptions()), NullLogger<PlannerService>.Instance);

        PlannerRowResponse result = await service.GetPlannerAsync(null, new[] { PlannerLevel.Sku, }, CancellationToken.None);

        PlannerRow skuUnitsRow = result.Data.Single(x => x.Level == PlannerLevel.Sku && x.SkuId == 1 && x.ValueType == PlannerValueType.Units);

        Assert.Equal(20m, skuUnitsRow.HistoryY0);
    }

    [Fact]
    public async Task Price_ShouldBeAmountDivUnits() {
        PlannerSourceSnapshot snapshot = BuildSimpleSnapshot();
        FakePlannerSnapshotProvider snapshotProvider = new(snapshot);

        FakePlanningY1Repository planningRepo = new(snapshot.PlanningBySkuSubId.Values.ToList());
        FakeSkuSubRepository skuSubRepo = new(snapshot.SkuSubs);

        PlannerService service = new(snapshotProvider, planningRepo, skuSubRepo, new PlannerMetadataBuilder(Options.Create(new PlannerOptions())), Options.Create(new PlannerOptions()), NullLogger<PlannerService>.Instance);

        PlannerRowResponse result = await service.GetPlannerAsync(null, new[] { PlannerLevel.Sku, }, CancellationToken.None);

        PlannerRow skuPriceRow = result.Data.Single(x => x.Level == PlannerLevel.Sku && x.SkuId == 1 && x.ValueType == PlannerValueType.Price);

        Assert.Equal(15m, skuPriceRow.HistoryY0);
    }

    [Fact]
    public async Task Filter_ShouldExcludeSkuSub_FromAllLevels() {
        PlannerSourceSnapshot snapshot = BuildSimpleSnapshot();
        FakePlannerSnapshotProvider snapshotProvider = new(snapshot);

        FakePlanningY1Repository planningRepo = new(snapshot.PlanningBySkuSubId.Values.ToList());
        FakeSkuSubRepository skuSubRepo = new(snapshot.SkuSubs);

        PlannerService service = new(snapshotProvider, planningRepo, skuSubRepo, new PlannerMetadataBuilder(Options.Create(new PlannerOptions())), Options.Create(new PlannerOptions()), NullLogger<PlannerService>.Instance);

        PlannerRowResponse result = await service.GetPlannerAsync(new[] { "SubA", }, new[] { PlannerLevel.Sku, }, CancellationToken.None);

        PlannerRow skuAmountRow = result.Data.Single(x => x.Level == PlannerLevel.Sku && x.SkuId == 1 && x.ValueType == PlannerValueType.Amount);

        Assert.Equal(100m, skuAmountRow.HistoryY0);
    }

    private static PlannerSourceSnapshot BuildSimpleSnapshot() {
        List<SkuEntity> skus = new() {
            new() {
                SkuId = 1,
                SkuName = "Sku1"
            }
        };

        List<SkuSubEntity> skuSubs = new() {
            new() {
                SkuSubId = 1,
                SkuId = 1,
                SkuSubName = "SubA",
                SkuPrice = 2m,
                SkuRatio = 1m
            },
            new() {
                SkuSubId = 2,
                SkuId = 1,
                SkuSubName = "SubB",
                SkuPrice = 3m,
                SkuRatio = 0.5m
            }
        };

        List<HistoryY0Entity> history = new() {
            new() {
                SkuSubId = 1,
                Units = 10m,
                Amount = 100m
            },
            new() {
                SkuSubId = 2,
                Units = 20m,
                Amount = 200m
            }
        };

        List<PlanningY1Entity> planning = new() {
            new() {
                SkuSubId = 1,
                Units = 12m,
                Amount = 24m
            },
            new() {
                SkuSubId = 2,
                Units = 21m,
                Amount = 63m
            }
        };

        Dictionary<int, SkuEntity> skuById = skus.ToDictionary(x => x.SkuId);
        Dictionary<int, SkuSubEntity> skuSubById = skuSubs.ToDictionary(x => x.SkuSubId);
        Dictionary<int, IReadOnlyList<SkuSubEntity>> skuSubsBySkuId = skuSubs.GroupBy(x => x.SkuId).ToDictionary(g => g.Key, g => (IReadOnlyList<SkuSubEntity>)g.ToList());

        Dictionary<string, SkuSubEntity> skuSubByName = skuSubs.GroupBy(x => x.SkuSubName, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        Dictionary<int, HistoryY0Entity> historyBySub = history.ToDictionary(x => x.SkuSubId);
        Dictionary<int, PlanningY1Entity> planningBySub = planning.ToDictionary(x => x.SkuSubId);

        return new(skus, skuSubs, history, planning, skuById, skuSubById, skuSubsBySkuId, skuSubByName, historyBySub, planningBySub);
    }

    private sealed class FakePlanningY1Repository : IPlanningY1Repository {
        private readonly Dictionary<int, PlanningY1Entity> _items;

        public FakePlanningY1Repository(IReadOnlyList<PlanningY1Entity> planning) {
            _items = planning.ToDictionary(x => x.SkuSubId);
        }

        public Task<IReadOnlyList<PlanningY1Entity>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default) {
            return Task.FromResult((IReadOnlyList<PlanningY1Entity>)_items.Values.ToList());
        }

        public Task UpsertAsync(int skuSubId, decimal units, decimal amount, CancellationToken ct = default) {
            throw new NotImplementedException();
        }

        public Task<PlanningY1Entity?> GetAsync(Expression<Func<PlanningY1Entity, bool>> filter, bool asNoTracking = true, CancellationToken ct = default, params Expression<Func<PlanningY1Entity, object>>[] includes) {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<PlanningY1Entity>> GetListAsync(Expression<Func<PlanningY1Entity, bool>>? filter = null, bool asNoTracking = true, CancellationToken ct = default, params Expression<Func<PlanningY1Entity, object>>[] includes) {
            throw new NotImplementedException();
        }

        public ValueTask<PlanningY1Entity?> FindAsync(object[] keyValues, CancellationToken ct = default) {
            int id = (int)keyValues[0];
            _items.TryGetValue(id, out PlanningY1Entity? entity);

            return ValueTask.FromResult(entity);
        }

        public Task AddAsync(PlanningY1Entity entity, CancellationToken ct = default) {
            throw new NotImplementedException();
        }

        public void Update(PlanningY1Entity entity) {
            _items[entity.SkuSubId] = entity;
        }

        public void Remove(PlanningY1Entity entity) {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) {
            return Task.FromResult(1);
        }
    }

    private sealed class FakePlannerSnapshotProvider(PlannerSourceSnapshot snapshot) : IPlannerSnapshotProvider {
        public Task<PlannerSourceSnapshot> GetAsync(CancellationToken ct = default) {
            return Task.FromResult(snapshot);
        }

        public void Invalidate() {}
    }

    private sealed class FakeSkuSubRepository(IReadOnlyList<SkuSubEntity> items) : ISkuSubRepository {
        private readonly List<SkuSubEntity> _items = items.ToList();

        public Task<IReadOnlyList<SkuSubEntity>> GetAllAsync(bool asNoTracking = true, CancellationToken ct = default) {
            return Task.FromResult((IReadOnlyList<SkuSubEntity>)_items);
        }

        public Task<IReadOnlyList<SkuSubEntity>> GetByNamesAsync(IReadOnlyCollection<string> skuSubNames, bool asNoTracking = true, CancellationToken ct = default) {
            throw new NotImplementedException();
        }

        public Task<SkuSubEntity?> GetAsync(Expression<Func<SkuSubEntity, bool>> filter, bool asNoTracking = true, CancellationToken ct = default, params Expression<Func<SkuSubEntity, object>>[] includes) {
            Func<SkuSubEntity, bool> compiled = filter.Compile();

            return Task.FromResult(_items.FirstOrDefault(compiled));
        }

        public Task<IReadOnlyList<SkuSubEntity>> GetListAsync(Expression<Func<SkuSubEntity, bool>>? filter = null, bool asNoTracking = true, CancellationToken ct = default, params Expression<Func<SkuSubEntity, object>>[] includes) {
            throw new NotImplementedException();
        }

        public ValueTask<SkuSubEntity?> FindAsync(object[] keyValues, CancellationToken ct = default) {
            throw new NotImplementedException();
        }

        public Task AddAsync(SkuSubEntity entity, CancellationToken ct = default) {
            throw new NotImplementedException();
        }

        public void Update(SkuSubEntity entity) {
            throw new NotImplementedException();
        }

        public void Remove(SkuSubEntity entity) {
            throw new NotImplementedException();
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) {
            throw new NotImplementedException();
        }
    }
}