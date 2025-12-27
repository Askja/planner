namespace Data.Abstractions;

using System.Linq.Expressions;
using EFCoreSecondLevelCacheInterceptor;
using Interfaces;
using Microsoft.EntityFrameworkCore;

public class BaseRepository<TEntity>(DataContext context) : IBaseRepository<TEntity> where TEntity : BaseEntity {
    protected DataContext Context { get; } = context;

    protected DbSet<TEntity> Set {
        get => context.Set<TEntity>();
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter, bool asNoTracking = true, CancellationToken ct = default, params Expression<Func<TEntity, object>>[] includes) {
        IQueryable<TEntity> q = BuildQuery(filter, asNoTracking, includes);

        return await q.FirstOrDefaultAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? filter = null, bool asNoTracking = true, CancellationToken ct = default, params Expression<Func<TEntity, object>>[] includes) {
        IQueryable<TEntity> q = BuildQuery(filter, asNoTracking, includes);

        return await q.ToListAsync(ct).ConfigureAwait(false);
    }

    public ValueTask<TEntity?> FindAsync(object[] keyValues, CancellationToken ct = default) {
        return Set.FindAsync(keyValues, ct);
    }

    public async Task AddAsync(TEntity entity, CancellationToken ct = default) {
        if (entity is null) {
            throw new ArgumentNullException(nameof(entity));
        }

        await Set.AddAsync(entity, ct).ConfigureAwait(false);
    }

    public void Update(TEntity entity) {
        if (entity is null) {
            throw new ArgumentNullException(nameof(entity));
        }

        Set.Update(entity);
    }

    public void Remove(TEntity entity) {
        if (entity is null) {
            throw new ArgumentNullException(nameof(entity));
        }

        Set.Remove(entity);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) {
        return Context.SaveChangesAsync(ct);
    }

    protected IQueryable<TEntity> BuildQuery(Expression<Func<TEntity, bool>>? filter, bool asNoTracking, params Expression<Func<TEntity, object>>[] includes) {
        IQueryable<TEntity> q = asNoTracking ? Set.AsNoTracking() : Set.AsTracking();

        if (asNoTracking) {
            q = q.Cacheable();
        }

        if (filter is not null) {
            q = q.Where(filter);
        }

        if (includes is { Length: > 0, }) {
            q = includes.Aggregate(q, (curr, inc) => curr.Include(inc));
            q = q.AsSplitQuery();
        }

        q = q.TagWith($"entity={typeof(TEntity).Name}");

        return q;
    }
}