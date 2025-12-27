namespace Data.Snapshots;

using Entities;

public sealed record PlannerSourceSnapshot(IReadOnlyList<SkuEntity> Skus, IReadOnlyList<SkuSubEntity> SkuSubs, IReadOnlyList<HistoryY0Entity> History, IReadOnlyList<PlanningY1Entity> Planning, IReadOnlyDictionary<int, SkuEntity> SkuById, IReadOnlyDictionary<int, SkuSubEntity> SkuSubById, IReadOnlyDictionary<int, IReadOnlyList<SkuSubEntity>> SkuSubsBySkuId, IReadOnlyDictionary<string, SkuSubEntity> SkuSubByName, IReadOnlyDictionary<int, HistoryY0Entity> HistoryBySkuSubId, IReadOnlyDictionary<int, PlanningY1Entity> PlanningBySkuSubId);