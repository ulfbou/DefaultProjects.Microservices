using DefaultProjects.Shared.DTOs;
using DefaultProjects.Shared.Models;
using DefaultProjects.Shared.Options;

namespace DefaultProjects.Shared.Interfaces;

public interface ITenantService
{
    Task<Tenant> CreateAsync(TenantCreationDTO tenantDto, CancellationToken cancellationToken);
    Task<Tenant?> GetAsync(string tenantId, CancellationToken cancellationToken);
    Task<Tenant?> UpdateAsync(string tenantId, TenantCreationDTO tenantDto, CancellationToken cancellationToken);
    Task DeleteAsync(string tenantId, CancellationToken cancellationToken);
}
