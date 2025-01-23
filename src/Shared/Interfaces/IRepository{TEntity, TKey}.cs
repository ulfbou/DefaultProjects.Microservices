using DefaultProjects.Shared.Options;

namespace DefaultProjects.Shared.Interfaces;

public interface IRepository<TEntity, TKey>
    where TEntity : class, ITenantEntity<TKey>
    where TKey : notnull, IEquatable<TKey>
{
    Task CreateAsync(string tenantId, TEntity entity, CancellationToken cancellationToken);
    Task<TEntity?> TryGetAsync(string tenantId, TKey id, RepositoryOptions<TEntity, TKey>? options, CancellationToken cancellationToken);
    Task UpdateAsync(string tenantId, TEntity entity, CancellationToken cancellationToken);
    Task DeleteAsync(string tenantId, TKey id, CancellationToken cancellationToken);
}
