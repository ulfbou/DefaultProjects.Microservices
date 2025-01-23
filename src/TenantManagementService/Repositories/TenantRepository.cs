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

    protected async Task ExecuteWithinTransactionAsync<TParameter>(Func<TParameter, CancellationToken, Task> action, string? tenantId, TParameter parameter, CancellationToken cancellationToken)
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
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

        _logger.LogInformation("Creating Tenant Id {TenantId}", tenant.TenantId);

        await ExecuteWithinTransactionAsync(CreateTenantAsync, null, tenant, cancellationToken);
    }

    private async Task CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        var existingTenant = await _context.Set<Tenant>().FindAsync(new object[] { tenant.TenantId }, cancellationToken);
        if (existingTenant != null)
        {
            _logger.LogInformation("Tenant Id {TenantId} already exists", tenant.TenantId);
            return;
        }

        _context.Set<Tenant>().Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string tenantId, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        _logger.LogInformation("Deleting tenant Id {TenantId}", tenantId);

        await ExecuteWithinTransactionAsync(DeleteTenantAsync, tenantId, tenantId, cancellationToken);
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

    public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        Guard.NotNull(tenant, nameof(tenant));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        _logger.LogInformation("Updating Tenant Id {TenantId}", tenant.TenantId);

        await ExecuteWithinTransactionAsync(UpdateTenantAsync, tenant.TenantId, tenant, cancellationToken);
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
}
