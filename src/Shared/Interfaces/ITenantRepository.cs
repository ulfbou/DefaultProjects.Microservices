using DefaultProjects.Shared.Models;
using DefaultProjects.Shared.Options;

namespace DefaultProjects.Shared.Interfaces;

public interface ITenantRepository
{
    Task CreateAsync(Tenant tenant, CancellationToken cancellationToken);
    Task<Tenant?> TryGetAsync(string id, RepositoryOptions? options, CancellationToken cancellationToken);
    Task UpdateAsync(Tenant entity, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
}
