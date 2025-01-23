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
    private readonly IContextObject _contextObject;
    private readonly ILogger<TenantRepository<TContext>> _logger;

    public TenantRepository(TContext context, IContextObject contextObject, ILogger<TenantRepository<TContext>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _contextObject = contextObject ?? throw new ArgumentNullException(nameof(contextObject));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected async Task ExecuteWithinTransactionAsync<TParameter>(Func<TParameter, CancellationToken, Task> action, TParameter parameter, CancellationToken cancellationToken)
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            string? tenantId = parameter switch
            {
                Tenant tenant => tenant.TenantId,
                string id => id,
                _ => null
            };

            try
            {

                await action(parameter, cancellationToken);
                scope.Complete();
            }
            catch (DbUpdateConcurrencyException ex) when (tenantId is not null)
            {
                _logger.LogError(ex, "Concurrency conflict when updating Tenant Id {TenantId}", tenantId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed");
                throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task CreateAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        Guard.NotNull(tenant, nameof(tenant));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        _logger.LogInformation("Creating Tenant Id {TenantId}", tenant.TenantId);

        await ExecuteWithinTransactionAsync(CreateTenantAsync, tenant, cancellationToken);
    }

    private async Task CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        var existingTenant = await _context.Set<Tenant>().FindAsync(new object[] { tenant.TenantId }, cancellationToken);

        if (existingTenant is not null)
        {
            _logger.LogInformation("Tenant Id {TenantId} already exists", tenant.TenantId);
            return;
        }

        _context.Set<Tenant>().Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Tenant?> TryGetAsync(string tenantId, RepositoryOptions<Tenant, string>? options, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        options ??= RepositoryOptions<Tenant, string>.Default;

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

    /// <inheritdoc />
    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        Guard.NotNull(tenant, nameof(tenant));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        _logger.LogInformation("Updating Tenant Id {TenantId}", tenant.TenantId);

        await ExecuteWithinTransactionAsync(UpdateTenantAsync, tenant, cancellationToken);
    }

    private async Task UpdateTenantAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        var existingTenant = await _context.Set<Tenant>().FindAsync(new object[] { tenant.TenantId }, cancellationToken);

        if (existingTenant is null)
        {
            _logger.LogWarning("Tenant Id {TenantId} not found", tenant.TenantId);
            return;
        }

        _context.Entry(tenant).Property("RowVersion").OriginalValue = tenant.RowVersion;
        _context.Set<Tenant>().Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string tenantId, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        _logger.LogInformation("Deleting tenant Id {TenantId}", tenantId);

        await ExecuteWithinTransactionAsync(DeleteTenantAsync, tenantId, cancellationToken);
    }

    private async Task DeleteTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        var tenant = await TryGetAsync(tenantId, default, cancellationToken);

        if (tenant is null)
        {
            _logger.LogWarning("Tenant Id {TenantId} not found", tenantId);
            return;
        }

        _context.Set<Tenant>().Remove(tenant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateAsync(IEnumerable<Tenant> tenants, CancellationToken cancellationToken)
    {
        Guard.NotNull(tenants, nameof(tenants));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        _logger.LogInformation("Creating {Count} Tenants", tenants.Count());

        await ExecuteWithinTransactionAsync(CreateTenantsAsync, tenants, cancellationToken);
    }

    private async Task CreateTenantsAsync(IEnumerable<Tenant> tenants, CancellationToken cancellationToken)
    {
        var existingTenantIds = await _context.Set<Tenant>()
            .Where(t => tenants.Select(x => x.TenantId).Contains(t.TenantId))
            .Select(t => t.TenantId)
            .ToListAsync(cancellationToken);

        var tenantsToCreate = tenants.Where(t => !existingTenantIds.Contains(t.TenantId));

        _context.Set<Tenant>().AddRange(tenantsToCreate);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Tenant>> TryGetAsync(IEnumerable<string> tenantIds, RepositoryOptions<Tenant, string>? options, CancellationToken cancellationToken)
    {
        Guard.NotNull(tenantIds, nameof(tenantIds));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        options ??= RepositoryOptions<Tenant, string>.Default;

        _logger.LogInformation("Getting {Count} Tenants", tenantIds.Count());

        try
        {
            var query = _context.Set<Tenant>()
                                .AsQueryable()
                                .ApplyOptions(options)
                                .Where(t => tenantIds.Contains(t.TenantId));

            return await query.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Tenants");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(IEnumerable<Tenant> tenants, CancellationToken cancellationToken)
    {
        Guard.NotNull(tenants, nameof(tenants));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        _logger.LogInformation("Updating {Count} Tenants", tenants.Count());

        await ExecuteWithinTransactionAsync(UpdateTenantsAsync, tenants, cancellationToken);
    }

    private async Task UpdateTenantsAsync(IEnumerable<Tenant> tenants, CancellationToken cancellationToken)
    {
        foreach (var tenant in tenants)
        {
            _context.Entry(tenant).Property("RowVersion").OriginalValue = tenant.RowVersion;
        }

        _context.Set<Tenant>().UpdateRange(tenants);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(IEnumerable<string> tenantIds, CancellationToken cancellationToken)
    {
        Guard.NotNull(tenantIds, nameof(tenantIds));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        _logger.LogInformation("Deleting {Count} Tenants", tenantIds.Count());

        await ExecuteWithinTransactionAsync(DeleteTenantsAsync, tenantIds, cancellationToken);
    }

    private async Task DeleteTenantsAsync(IEnumerable<string> tenantIds, CancellationToken cancellationToken)
    {
        var tenantsToDelete = await _context.Set<Tenant>()
            .Where(t => tenantIds.Contains(t.TenantId))
            .ToListAsync(cancellationToken);

        _context.Set<Tenant>().RemoveRange(tenantsToDelete);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
