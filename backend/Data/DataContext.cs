namespace Data;

using Abstractions;
using Configurations;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public sealed class DataContext(DbContextOptions<DataContext> options) : DbContext(options) {
    public DbSet<SkuEntity> TableSku {
        get => Set<SkuEntity>();
    }

    public DbSet<SkuSubEntity> TableSkuSub {
        get => Set<SkuSubEntity>();
    }

    public DbSet<HistoryY0Entity> TableHistoryY0 {
        get => Set<HistoryY0Entity>();
    }

    public DbSet<PlanningY1Entity> TablePlanningY1 {
        get => Set<PlanningY1Entity>();
    }

    public override int SaveChanges() {
        UpdateTimestamps();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
        UpdateTimestamps();

        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps() {
        foreach (EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>()) {
            if (entry.State == EntityState.Modified) {
                entry.Property(e => e.CreatedAt).IsModified = false;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            } else if (entry.State == EntityState.Added) {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
        configurationBuilder.Properties<string>().HaveMaxLength(200);
        configurationBuilder.Properties<decimal>().HavePrecision(18, 6);

        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyConfiguration(new SkuConfiguration());
        modelBuilder.ApplyConfiguration(new SkuSubConfiguration());
        modelBuilder.ApplyConfiguration(new HistoryY0Configuration());
        modelBuilder.ApplyConfiguration(new PlanningY1Configuration());

        base.OnModelCreating(modelBuilder);
    }
}