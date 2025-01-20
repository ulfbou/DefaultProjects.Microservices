using DefaultProjects.Shared.Extensions;
using DefaultProjects.Shared.Interfaces;
using DefaultProjects.Shared.Models;
using DefaultProjects.Shared.Options;
using FluentInjections.Validation;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

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

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {

                var existingTenant = await _context.Set<Tenant>().FindAsync(new object[] { tenant.TenantId }, cancellationToken);
                if (existingTenant != null)
                {
                    _logger.LogInformation("Tenant Id {TenantId} already exists", tenant.TenantId);
                    return;
                }
         _context.Entry(tenant).Property("RowVersion").OriginalValue = tenant.RowVersion;
            _context.Set<Tenant>().Update(tenant);
            await _context.SaveChangesAsync(cancellationToken);
            scope.Complete();
                _context.Set<Tenant>().Add(tenant);
                await _context.SaveChangesAsync(cancellationToken);
                scope.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Tenant Id {TenantId}", tenant.TenantId);
                throw;
            }
        }
    }

    public async Task DeleteAsync(string tenantId, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        _logger.LogInformation("Deleting tenant Id {TenantId}", tenantId);

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var tenant = await TryGetAsync(tenantId: tenantId, RepositoryOptions.Default, cancellationToken: cancellationToken);

                if (tenant is null)
                {
                    _logger.LogWarning("Tenant Id {TenantId} not found", tenantId);
                    return;
                }

                _context.Set<Tenant>().Remove(tenant);
                await _context.SaveChangesAsync(cancellationToken);
                scope.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete Tenant Id {TenantId}", tenantId);
                throw;
            }
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

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                _context.Entry(tenant).Property("RowVersion").OriginalValue = tenant.RowVersion;
                _context.Set<Tenant>().Update(tenant);
                await _context.SaveChangesAsync(cancellationToken);
                scope.Complete();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict when updating Tenant Id {TenantId}", tenant.TenantId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Tenant Id {TenantId}", tenant.TenantId);
                throw;
            }
        }
    }
}