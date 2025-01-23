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

    public async Task CreateAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        Guard.NotNull(tenant, nameof(tenant));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        if (!string.IsNullOrWhiteSpace(tenant.TenantId))
        {
            throw new ArgumentException("Tenant Id must not be set", nameof(tenant));
        }

        _logger.LogInformation("Creating Tenant Id {TenantId}", tenant.TenantId);

        await ExecuteWithinTransactionAsync(CreateTenantAsync, tenant, cancellationToken);
    }

    private async Task CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        var existingTenant = await _context.Set<Tenant>()
                                           .FindAsync(new object[] { tenant.TenantId }, cancellationToken);

        if (existingTenant is not null)
        {
            _logger.LogInformation("Tenant Id {TenantId} already exists", tenant.TenantId);
            return;
        }

        _context.Set<Tenant>().Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Tenant?> TryGetAsync(string tenantId, RepositoryOptions<Tenant, string>? options, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        options ??= RepositoryOptions<Tenant, string>.Default;

        _logger.LogInformation("Getting Tenant Id {TenantId}", tenantId);

        try
        {
            return await _context.Set<Tenant>()
                                 .AsQueryable()
                                 .ApplyOptions(options)
                                 .FirstOrDefaultAsync(e => e.TenantId == tenantId, cancellationToken);
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

        if (string.IsNullOrWhiteSpace(tenant.TenantId))
        {
            throw new ArgumentException("Tenant Id is required", nameof(tenant));
        }

        _logger.LogInformation("Updating Tenant Id {TenantId}", tenant.TenantId);

        await ExecuteWithinTransactionAsync(UpdateTenantAsync, tenant, cancellationToken);
    }

    private async Task UpdateTenantAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        var existingTenant = await _context.Set<Tenant>().FindAsync(new object[] { tenant.TenantId }, cancellationToken);

        if (existingTenant is null)
        {
            _logger.LogWarning("Update failed: Tenant Id {TenantId} not found. Unable to update the tenant because it does not exist in the database.", tenant.TenantId);
            throw new InvalidOperationException($"Update failed: Tenant Id {tenant.TenantId} not found.");
        }

        _context.Entry(tenant).Property("RowVersion").OriginalValue = tenant.RowVersion;
        _context.Set<Tenant>().Update(tenant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // TODO: Implement soft delete options
    public async Task DeleteAsync(string tenantId, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        _logger.LogInformation("Deleting Tenant Id {TenantId}", tenantId);

        await ExecuteWithinTransactionAsync(DeleteTenantAsync, tenantId, cancellationToken);
    }

    private async Task DeleteTenantAsync(string tenantId, CancellationToken cancellationToken)
    {
        var tenant = await _context.Set<Tenant>().FindAsync(new object[] { tenantId }, cancellationToken);

        if (tenant is null)
        {
            _logger.LogWarning("Delete failed: Tenant Id {TenantId} not found. Unable to delete the tenant because it does not exist in the database.", tenantId);
            throw new InvalidOperationException($"Delete failed: Tenant Id {tenantId} not found.");
        }

        _context.Set<Tenant>().Remove(tenant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateAsync(IEnumerable<Tenant> tenants, CancellationToken cancellationToken)
    {
        Guard.NotNull(tenants, nameof(tenants));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        if (tenants.Where(t => !string.IsNullOrWhiteSpace(t.TenantId)).Any())
        {
            throw new ArgumentException("Create failed: TenantId must not be empty", nameof(tenants));
        }

        var duplicateTenantIds = tenants.GroupBy(t => t.TenantId)
                                        .Where(g => g.Count() > 1)
                                        .Select(g => g.Key);

        if (duplicateTenantIds.Any())
        {
            throw new ArgumentException($"Create failed: TenantId must be unique. Duplicates found: {string.Join(", ", duplicateTenantIds)}", nameof(tenants));
        }

        _logger.LogInformation("Creating {Count} Tenants", tenants.Count());

        await ExecuteWithinTransactionAsync(CreateTenantsAsync, tenants, cancellationToken);
    }

    private async Task CreateTenantsAsync(IEnumerable<Tenant> tenants, CancellationToken cancellationToken)
    {
        _context.Set<Tenant>().AddRange(tenants);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tenant>> TryGetAsync(IEnumerable<string> tenantIds, RepositoryOptions<Tenant, string>? options, CancellationToken cancellationToken)
    {
        ValidateParameters("Get", tenantIds, cancellationToken);

        options ??= RepositoryOptions<Tenant, string>.Default;

        _logger.LogInformation("Getting {Count} Tenants", tenantIds.Count());

        try
        {
            return await _context.Set<Tenant>()
                                 .AsQueryable()
                                 .ApplyOptions(options)
                                 .Where(t => tenantIds.Contains(t.TenantId))
                                 .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Tenants");
            throw;
        }
    }

    public async Task UpdateAsync(IEnumerable<Tenant> tenants, CancellationToken cancellationToken)
    {
        ValidateParameters("Update", tenants.Select(t => t.TenantId), cancellationToken);

        _logger.LogInformation("Updating {Count} Tenants", tenants.Count());

        await ExecuteWithinTransactionAsync(UpdateTenantsAsync, tenants, cancellationToken);
    }

    private async Task UpdateTenantsAsync(IEnumerable<Tenant> tenants, CancellationToken cancellationToken)
    {
        var existingTenants = await _context.Set<Tenant>()
                                            .AsTracking()
                                            .Where(t => tenants.Select(x => x.TenantId).Contains(t.TenantId))
                                            .ToListAsync(cancellationToken);

        var missingTenants = tenants.Where(t => !existingTenants.Any(et => et.TenantId == t.TenantId));

        if (missingTenants.Any())
        {
            var missingTenantIds = missingTenants.Select(t => t.TenantId);
            _logger.LogWarning("Update failed: TenantIds not found. Unable to update the following tenants because they do not exist in the database: {TenantIds}", string.Join(", ", missingTenantIds));
            throw new InvalidOperationException($"Update failed: TenantIds not found. Unable to update the following tenants because they do not exist in the database: {string.Join(", ", missingTenantIds)}");
        }

        foreach (var tenant in tenants)
        {
            _context.Entry(tenant).Property("RowVersion").OriginalValue = tenant.RowVersion;
        }

        _context.Set<Tenant>().UpdateRange(tenants);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(IEnumerable<string> tenantIds, CancellationToken cancellationToken)
    {
        ValidateParameters("Delete", tenantIds, cancellationToken);

        _logger.LogInformation("Deleting {Count} Tenants", tenantIds.Count());

        await ExecuteWithinTransactionAsync(DeleteTenantsAsync, tenantIds, cancellationToken);
    }

    private async Task DeleteTenantsAsync(IEnumerable<string> tenantIds, CancellationToken cancellationToken)
    {
        var tenantsToDelete = await _context.Set<Tenant>()
                                            .AsNoTracking()
                                            .Where(t => tenantIds.Contains(t.TenantId))
                                            .ToListAsync(cancellationToken);

        _context.Set<Tenant>().RemoveRange(tenantsToDelete);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private void ValidateParameters(string operationName, IEnumerable<string> tenantIds, CancellationToken cancellationToken)
    {
        Guard.NotNull(tenantIds, nameof(tenantIds));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        var duplicateTenantIds = tenantIds.Where(t => !string.IsNullOrWhiteSpace(t))
                                          .GroupBy(t => t)
                                          .Where(g => g.Count() > 1)
                                          .Select(g => g.Key);

        if (duplicateTenantIds.Any())
        {
            _logger.LogWarning("{Operation} failed: Duplicate TenantIds must be unique. Duplicates found: {TenantIds}", operationName, string.Join(", ", duplicateTenantIds));
            throw new ArgumentException($"{operationName} failed: TenantIds must be unique. Duplicates found: {string.Join(", ", duplicateTenantIds)}", nameof(tenantIds));
        }
    }
}
