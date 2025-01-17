using DefaultProjects.Shared.Extensions;
using DefaultProjects.Shared.Interfaces;
using DefaultProjects.Shared.Models;
using DefaultProjects.Shared.Options;

using FluentInjections.Validation;

using Microsoft.EntityFrameworkCore;

namespace DefaultProjects.Microservices.TenantManagementServices.Repositories;

public class TenantRepository<TContext> : ITenantRepository where TContext : DbContext
{
    private readonly TContext _context;
    private readonly ILogger<TenantRepository<TContext>> _logger;

    public TenantRepository(TContext context, ILogger<TenantRepository<TContext>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task CreateAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        Guard.NotNull(tenant, nameof(tenant));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        _logger.LogInformation("Creating Tenant Id {TenantId}", tenant.TenantId);
        try
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                _context.Set<Tenant>().Add(tenant);
                await _context.SaveChangesAsync(cancellationToken);
                transaction.Commit();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Tenant Id {TenantId}", tenant.TenantId);
            throw;
        }
    }

    public async Task DeleteAsync(string tenantId, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        _logger.LogInformation("Deleting tenant Id {TenantId}", tenantId);

        try
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                var tenant = await TryGetAsync(tenantId: tenantId, RepositoryOptions.Default, cancellationToken: cancellationToken);

                if (tenant is null)
                {
                    _logger.LogWarning("Tenant Id {TenantId} not found", tenantId);
                    transaction.Rollback();
                    return;
                }

                _context.Set<Tenant>().Remove(tenant);
                await _context.SaveChangesAsync(cancellationToken);
                transaction.Commit();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete Tenant Id {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<Tenant?> TryGetAsync(string tenantId, RepositoryOptions? options, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        options ??= RepositoryOptions.Default;

        _logger.LogInformation("Getting Tenant Id {TenantId}", tenantId);

        try
        {
            var query = _context.Set<Tenant>()
                                .AsQueryable()
                                .ApplyOptions(options);

            return await query.FirstOrDefaultAsync(e => e.TenantId == tenantId, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Tenant Id {TenantId}", tenantId);
            throw;
        }
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        Guard.NotNull(tenant, nameof(tenant));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        _logger.LogInformation("Updating Tenant Id {TenantId}", tenant.TenantId);

        try
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                _context.Set<Tenant>().Update(tenant);
                await _context.SaveChangesAsync(cancellationToken);
                transaction.Commit();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Tenant Id {TenantId}", tenant.TenantId);
            throw;
        }
    }
}
