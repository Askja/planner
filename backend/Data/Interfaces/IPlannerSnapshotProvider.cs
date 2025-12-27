namespace Data.Interfaces;

using Snapshots;

public interface IPlannerSnapshotProvider {
    Task<PlannerSourceSnapshot> GetAsync(CancellationToken ct = default);
    void Invalidate();
}