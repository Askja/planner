namespace Data.Interfaces;

using System.Linq.Expressions;
using Abstractions;

public interface IBaseRepository<TEntity> where TEntity : BaseEntity {
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filter, bool asNoTracking = true, CancellationToken ct = default, params Expression<Func<TEntity, object>>[] includes);
    Task<IReadOnlyList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? filter = null, bool asNoTracking = true, CancellationToken ct = default, params Expression<Func<TEntity, object>>[] includes);
    ValueTask<TEntity?> FindAsync(object[] keyValues, CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}