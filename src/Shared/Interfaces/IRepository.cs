using DefaultProjects.Shared.Options;

namespace DefaultProjects.Shared.Interfaces;

public interface IRepository<TEntity> where TEntity : class, ITenantEntity
{
    Task CreateAsync(string tenantId, TEntity entity, CancellationToken cancellationToken);
    Task<TEntity?> TryGetAsync(string tenantId, string id, RepositoryOptions? options, CancellationToken cancellationToken);
    Task UpdateAsync(string tenantId, TEntity entity, CancellationToken cancellationToken);
    Task DeleteAsync(string tenantId, string id, CancellationToken cancellationToken);
}
