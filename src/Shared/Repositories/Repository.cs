using DefaultProjects.Shared.Extensions;
using DefaultProjects.Shared.Interfaces;
using DefaultProjects.Shared.Models;
using DefaultProjects.Shared.Options;

using FluentInjections.Validation;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DefaultProjects.Shared.Repositories;

public class Repository<TContext, TEntity> : IRepository<TEntity> where TContext : DbContext where TEntity : class, ITenantEntity
{
    private readonly TContext _context;
    private readonly ILogger<Repository<TContext, TEntity>> _logger;
    private readonly string _entityName;

    public Repository(TContext context, ILogger<Repository<TContext, TEntity>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _entityName = typeof(TEntity).Name;
    }

    protected async Task ExecuteWithinTransactionAsync<TParameter>(Func<TParameter, CancellationToken, Task> action, TParameter parameter, CancellationToken cancellationToken)
    {
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                await action(parameter, cancellationToken);
                scope.Complete();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transaction failed");
                throw;
            }
        }
    }

    public async Task CreateAsync(string tenantId, TEntity entity, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(entity, nameof(entity));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        ValidateTenant(tenantId, entity);
        _logger.LogInformation("Creating {EntityName} Id {EntityId} with TenantId {TenantId}", _entityName, entity.Id, tenantId);

        await ExecuteWithinTransactionAsync(CreateEntityAsync, (tenantId, entity), cancellationToken);
    }

    private async Task CreateEntityAsync((string tenantId, TEntity entity) parameters, CancellationToken cancellationToken)
    {
        var (tenantId, entity) = parameters;
        var existingEntity = await _context.Set<TEntity>().FindAsync(new object[] { entity.Id }, cancellationToken);
        if (existingEntity != null)
        {
            _logger.LogInformation("{EntityName} Id {EntityId} already exists with TenantId {TenantId}", _entityName, entity.Id, tenantId);
            return;
        }

        _context.Set<TEntity>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(string tenantId, string id, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
        Guard.NotNullOrEmpty(id, nameof(id));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        ValidateTenant(tenantId, id);
        _logger.LogInformation("Deleting {EntityName} Id {Id} with TenantId {TenantId}", _entityName, id, tenantId);

        await ExecuteWithinTransactionAsync(DeleteEntityAsync, (tenantId, id), cancellationToken);
    }

    private async Task DeleteEntityAsync((string tenantId, string id) parameters, CancellationToken cancellationToken)
    {
        var (tenantId, id) = parameters;
        var entity = await TryGetAsync(tenantId, id, RepositoryOptions.Default, cancellationToken);

        if (entity is null)
        {
            _logger.LogWarning("{EntityName} {Id} with TenantId {TenantId} not found", _entityName, id, tenantId);
            return;
        }

        _context.Set<TEntity>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TEntity?> TryGetAsync(string tenantId, string id, RepositoryOptions? options, CancellationToken cancellationToken)
    {
        Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        ValidateTenant(tenantId, id);
        options ??= RepositoryOptions.Default;

        _logger.LogInformation("Getting {EntityName} Id {Id} with TenantId {TenantId}", _entityName, id, tenantId);

        try
        {
            var query = _context.Set<TEntity>()
                                .AsQueryable()
                                .ApplyOptions(options);

            return await query.FirstOrDefaultAsync(e => e.Id == id && e.TenantId == tenantId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get {EntityName} Id {Id} with TenantId {TenantId}", _entityName, id, tenantId);
            throw;
        }
    }

    public async Task UpdateAsync(string tenantId, TEntity entity, CancellationToken cancellationToken)
    {
        Guard.NotNull(entity, nameof(entity));
        Guard.NotNull(cancellationToken, nameof(cancellationToken));

        ValidateTenant(tenantId, entity);
        _logger.LogInformation("Updating {EntityName} Id {EntityId} with TenantId {TenantId}", _entityName, entity.Id, tenantId);

        await ExecuteWithinTransactionAsync(UpdateEntityAsync, (tenantId, entity), cancellationToken);
    }

    private async Task UpdateEntityAsync((string tenantId, TEntity entity) parameters, CancellationToken cancellationToken)
    {
        var (tenantId, entity) = parameters;
        _context.Entry(entity).Property("RowVersion").OriginalValue = entity.RowVersion;
        _context.Set<TEntity>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Validates the TenantId of the TEntity with an expected TenantId.
    /// </summary>
    /// <param name="tenantId">The expected TenantId.</param>
    /// <param name="entity">The TEntity to validate.</param>
    /// <exception cref="InvalidOperationException">Thrown when the entity's TenantId does not match the expected TenantId.</exception>
    /// <remarks>If the entity's TenantId was not set, it will be set to the expected TenantId.</remarks>
    protected virtual void ValidateTenant(string tenantId, TEntity entity)
    {
        Guard.NotNullOrEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(entity, nameof(entity));

        if (!ValidateTenant(tenantId, entity.TenantId))
        {
            entity.TenantId = tenantId;
        }
    }

    /// <summary>
    /// Validates the TenantId of the TEntity with an expected TenantId. 
    /// </summary>
    /// <param name="expectedTenantId">The expected TenantId.</param>
    /// <param name="actualTenantId">The actual TenantId.</param>
    /// <returns><see langword="false"/> if the entity's TenantId was not set; otherwise, <see langword="true"/>, if .</returns>
    /// <returns><see langword="true"/> if the entity's TenantId matches the expected TenantId; otherwise, if the entity's TenantId was not set, <see langword="false"/>.</returns>"
    /// <exception cref="InvalidOperationException">Thrown when the entity's TenantId does not match the expected TenantId.</exception>
    protected virtual bool ValidateTenant(string expectedTenantId, string actualTenantId)
    {
        Guard.NotNullOrEmpty(expectedTenantId, nameof(expectedTenantId));
        Guard.NotNullOrEmpty(actualTenantId, nameof(actualTenantId));

        if (actualTenantId != expectedTenantId && actualTenantId != Guid.Empty.ToString())
        {
            if (actualTenantId != Guid.Empty.ToString())
            {
                throw new InvalidOperationException($"{_entityName} TenantId {actualTenantId} does not match expected TenantId {expectedTenantId}.");
            }

            return false;
        }

        return true;
    }
}
