using DefaultProjects.Shared.Models;
using DefaultProjects.Shared.Options;

namespace DefaultProjects.Shared.Interfaces;

public interface ITenantRepository
{
    Task CreateAsync(Tenant tenant, CancellationToken cancellationToken);
    Task<Tenant?> TryGetAsync(string tenantId, RepositoryOptions<Tenant, string>? options, CancellationToken cancellationToken);
    Task UpdateAsync(Tenant entity, CancellationToken cancellationToken);
    Task DeleteAsync(string id, CancellationToken cancellationToken);
    Task CreateAsync(IEnumerable<Tenant> tenants, CancellationToken cancellationToken);
    Task<IEnumerable<Tenant>> TryGetAsync(IEnumerable<string> tenantIds, RepositoryOptions<Tenant, string>? options, CancellationToken cancellationToken);
    Task UpdateAsync(IEnumerable<Tenant> tenants, CancellationToken cancellationToken);
    Task DeleteAsync(IEnumerable<string> tenantIds, CancellationToken cancellationToken);
}
