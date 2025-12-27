namespace Services.Services;

using Data.Entities;
using Data.Interfaces;
using Data.Snapshots;
using Domain.DTO.Request;
using Domain.DTO.Response;
using Domain.Enums;
using Domain.Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public sealed class PlannerService(IPlannerSnapshotProvider snapshotProvider, IPlanningY1Repository planningY1Repository, ISkuSubRepository skuSubRepository, IPlannerMetadataBuilder metadataBuilder, IOptions<PlannerOptions> options, ILogger<PlannerService> logger) : IPlannerService {
    private readonly PlannerOptions _options = options.Value;

    public async Task<PlannerRowResponse> GetPlannerAsync(IReadOnlyCollection<string>? skuSubNames, IReadOnlyCollection<PlannerLevel>? levels, CancellationToken ct = default) {
        PlannerLevel[] usedLevels = levels is null || levels.Count == 0 ? _options.DefaultLevels : levels.Distinct().ToArray();

        PlannerSourceSnapshot snapshot = await snapshotProvider.GetAsync(ct).ConfigureAwait(false);

        IReadOnlyList<SkuSubEntity> includedSkuSubs = FilterSkuSubs(snapshot, skuSubNames);
        HashSet<int> includedSkuSubIds = includedSkuSubs.Select(x => x.SkuSubId).ToHashSet();

        List<IGrouping<int, SkuSubEntity>> groupedBySkuId = includedSkuSubs.GroupBy(x => x.SkuId).ToList();
        List<int> skuIds = groupedBySkuId.Select(g => g.Key).ToList();

        Dictionary<int, Measures> skuMeasures = new(skuIds.Count);

        foreach (IGrouping<int, SkuSubEntity> group in groupedBySkuId) {
            int skuId = group.Key;

            decimal historyAmount = 0m;
            decimal historyUnits = 0m;

            decimal planningAmount = 0m;
            decimal planningUnits = 0m;

            foreach (SkuSubEntity sub in group) {
                snapshot.HistoryBySkuSubId.TryGetValue(sub.SkuSubId, out HistoryY0Entity? h);
                snapshot.PlanningBySkuSubId.TryGetValue(sub.SkuSubId, out PlanningY1Entity? p);

                decimal subHistoryUnits = h?.Units ?? 0m;
                decimal subHistoryAmount = h?.Amount ?? 0m;

                decimal subPlanningUnits = p?.Units ?? 0m;
                decimal subPlanningPrice = sub.SkuPrice;
                decimal subPlanningAmount = subPlanningUnits * subPlanningPrice;

                historyAmount += subHistoryAmount;
                historyUnits += subHistoryUnits * sub.SkuRatio;

                planningAmount += subPlanningAmount;
                planningUnits += subPlanningUnits * sub.SkuRatio;
            }

            decimal historyPrice = SafeDivide(historyAmount, historyUnits);
            decimal planningPrice = SafeDivide(planningAmount, planningUnits);

            skuMeasures[skuId] = new(historyUnits, historyPrice, historyAmount, planningUnits, planningPrice, planningAmount);
        }

        decimal totalHistoryAmount = 0m;
        decimal totalHistoryUnits = 0m;
        decimal totalPlanningAmount = 0m;
        decimal totalPlanningUnits = 0m;

        foreach (Measures m in skuMeasures.Values) {
            totalHistoryAmount += m.HistoryAmount;
            totalHistoryUnits += m.HistoryUnits;
            totalPlanningAmount += m.PlanningAmount;
            totalPlanningUnits += m.PlanningUnits;
        }

        decimal totalHistoryPrice = SafeDivide(totalHistoryAmount, totalHistoryUnits);
        decimal totalPlanningPrice = SafeDivide(totalPlanningAmount, totalPlanningUnits);

        Measures totalMeasures = new(totalHistoryUnits, totalHistoryPrice, totalHistoryAmount, totalPlanningUnits, totalPlanningPrice, totalPlanningAmount);

        List<PlannerRow> rows = new(2048);

        if (usedLevels.Contains(PlannerLevel.Total)) {
            AddTotalRows(rows, totalMeasures);
        }

        foreach (int skuId in skuIds.OrderBy(x => x)) {
            if (!snapshot.SkuById.TryGetValue(skuId, out SkuEntity? sku)) {
                continue;
            }

            string skuName = sku.SkuName;
            Measures skuM = skuMeasures[skuId];

            if (usedLevels.Contains(PlannerLevel.Sku)) {
                AddSkuRows(rows, skuId, skuName, skuM, totalMeasures);
            }

            if (usedLevels.Contains(PlannerLevel.SkuSub)) {
                List<SkuSubEntity> subs = groupedBySkuId.First(g => g.Key == skuId).OrderBy(x => x.SkuSubId).ToList();

                foreach (SkuSubEntity sub in subs) {
                    AddSkuSubRows(rows, sub, snapshot, skuM);
                }
            }
        }

        return new() {
            Data = rows,
            Metadata = metadataBuilder.Build()
        };
    }

    public async Task UpdatePlanningAsync(PlannerRowRequest request, CancellationToken ct = default) {
        if (request.SkuSubId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(request.SkuSubId));
        }

        if (request.PlanningUnits < 0m) {
            throw new ArgumentOutOfRangeException(nameof(request.PlanningUnits));
        }

        SkuSubEntity? skuSub = await skuSubRepository.GetAsync(x => x.SkuSubId == request.SkuSubId, true, ct).ConfigureAwait(false);

        if (skuSub is null) {
            throw new InvalidOperationException($"SkuSub not found. skuSubId={request.SkuSubId}");
        }

        decimal amount = request.PlanningUnits * skuSub.SkuPrice;

        PlanningY1Entity? planning = await planningY1Repository.FindAsync([request.SkuSubId,], ct).ConfigureAwait(false);

        if (planning is null) {
            throw new InvalidOperationException($"PlanningY1 row not found for skuSubId={request.SkuSubId}");
        }

        planning.Units = request.PlanningUnits;
        planning.Amount = amount;

        planningY1Repository.Update(planning);
        await planningY1Repository.SaveChangesAsync(ct).ConfigureAwait(false);

        snapshotProvider.Invalidate();

        logger.LogInformation("Planning updated: skuSubId={skuSubId} units={units} amount={amount}", request.SkuSubId, request.PlanningUnits, amount);
    }

    private static IReadOnlyList<SkuSubEntity> FilterSkuSubs(PlannerSourceSnapshot snapshot, IReadOnlyCollection<string>? skuSubNames) {
        if (skuSubNames is null || skuSubNames.Count == 0) {
            return snapshot.SkuSubs;
        }

        List<SkuSubEntity> result = new(skuSubNames.Count);

        foreach (string name in skuSubNames) {
            if (string.IsNullOrWhiteSpace(name)) {
                continue;
            }

            if (snapshot.SkuSubByName.TryGetValue(name.Trim(), out SkuSubEntity? sub)) {
                result.Add(sub);
            }
        }

        return result;
    }

    private static decimal SafeDivide(decimal numerator, decimal denominator) {
        if (denominator == 0m) {
            return 0m;
        }

        return numerator / denominator;
    }

    private static void AddTotalRows(List<PlannerRow> rows, Measures total) {
        rows.Add(
            new() {
                Level = PlannerLevel.Total,
                ValueType = PlannerValueType.Units,
                HistoryY0 = total.HistoryUnits,
                PlanningY1 = total.PlanningUnits,
                ContributionGrowth = SafeDivide(total.PlanningUnits - total.HistoryUnits, total.HistoryUnits)
            }
        );

        rows.Add(
            new() {
                Level = PlannerLevel.Total,
                ValueType = PlannerValueType.Price,
                HistoryY0 = total.HistoryPrice,
                PlanningY1 = total.PlanningPrice,
                ContributionGrowth = SafeDivide(total.PlanningPrice - total.HistoryPrice, total.HistoryPrice)
            }
        );

        rows.Add(
            new() {
                Level = PlannerLevel.Total,
                ValueType = PlannerValueType.Amount,
                HistoryY0 = total.HistoryAmount,
                PlanningY1 = total.PlanningAmount,
                ContributionGrowth = SafeDivide(total.PlanningAmount - total.HistoryAmount, total.HistoryAmount)
            }
        );
    }

    private static void AddSkuRows(List<PlannerRow> rows, int skuId, string skuName, Measures sku, Measures total) {
        rows.Add(
            new() {
                Level = PlannerLevel.Sku,
                ValueType = PlannerValueType.Units,
                SkuId = skuId,
                SkuName = skuName,
                HistoryY0 = sku.HistoryUnits,
                PlanningY1 = sku.PlanningUnits,
                ContributionGrowth = SafeDivide(sku.PlanningUnits - sku.HistoryUnits, total.HistoryUnits)
            }
        );

        rows.Add(
            new() {
                Level = PlannerLevel.Sku,
                ValueType = PlannerValueType.Price,
                SkuId = skuId,
                SkuName = skuName,
                HistoryY0 = sku.HistoryPrice,
                PlanningY1 = sku.PlanningPrice,
                ContributionGrowth = SafeDivide(sku.PlanningPrice - sku.HistoryPrice, total.HistoryPrice)
            }
        );

        rows.Add(
            new() {
                Level = PlannerLevel.Sku,
                ValueType = PlannerValueType.Amount,
                SkuId = skuId,
                SkuName = skuName,
                HistoryY0 = sku.HistoryAmount,
                PlanningY1 = sku.PlanningAmount,
                ContributionGrowth = SafeDivide(sku.PlanningAmount - sku.HistoryAmount, total.HistoryAmount)
            }
        );
    }

    private static void AddSkuSubRows(List<PlannerRow> rows, SkuSubEntity sub, PlannerSourceSnapshot snapshot, Measures skuMeasures) {
        snapshot.HistoryBySkuSubId.TryGetValue(sub.SkuSubId, out HistoryY0Entity? h);
        snapshot.PlanningBySkuSubId.TryGetValue(sub.SkuSubId, out PlanningY1Entity? p);

        decimal historyUnits = h?.Units ?? 0m;
        decimal historyAmount = h?.Amount ?? 0m;
        decimal historyPrice = SafeDivide(historyAmount, historyUnits);

        decimal planningUnits = p?.Units ?? 0m;
        decimal planningPrice = sub.SkuPrice;
        decimal planningAmount = planningUnits * planningPrice;

        rows.Add(
            new() {
                Level = PlannerLevel.SkuSub,
                ValueType = PlannerValueType.Units,
                SkuId = sub.SkuId,
                SkuSubId = sub.SkuSubId,
                SkuSubName = sub.SkuSubName,
                HistoryY0 = historyUnits,
                PlanningY1 = planningUnits,
                ContributionGrowth = SafeDivide(planningUnits - historyUnits, skuMeasures.HistoryUnits)
            }
        );

        rows.Add(
            new() {
                Level = PlannerLevel.SkuSub,
                ValueType = PlannerValueType.Price,
                SkuId = sub.SkuId,
                SkuSubId = sub.SkuSubId,
                SkuSubName = sub.SkuSubName,
                HistoryY0 = historyPrice,
                PlanningY1 = planningPrice,
                ContributionGrowth = SafeDivide(planningPrice - historyPrice, skuMeasures.HistoryPrice)
            }
        );

        rows.Add(
            new() {
                Level = PlannerLevel.SkuSub,
                ValueType = PlannerValueType.Amount,
                SkuId = sub.SkuId,
                SkuSubId = sub.SkuSubId,
                SkuSubName = sub.SkuSubName,
                HistoryY0 = historyAmount,
                PlanningY1 = planningAmount,
                ContributionGrowth = SafeDivide(planningAmount - historyAmount, skuMeasures.HistoryAmount)
            }
        );
    }

    private sealed record Measures(decimal HistoryUnits, decimal HistoryPrice, decimal HistoryAmount, decimal PlanningUnits, decimal PlanningPrice, decimal PlanningAmount);
}