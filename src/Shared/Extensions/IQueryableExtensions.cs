using Microsoft.EntityFrameworkCore;
using DefaultProjects.Shared.Options;
using DefaultProjects.Shared.Models;
using DefaultProjects.Shared.Interfaces;

namespace DefaultProjects.Shared.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int page, int pageSize)
    {
        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }

    // Apply RepositoryOptions to query
    public static IQueryable<TEntity> ApplyOptions<TEntity, TKey>(this IQueryable<TEntity> query, RepositoryOptions<TEntity, TKey> options)
        where TEntity : class, IEntity<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        return options.UseAsTracking ? query.AsTracking<TEntity>() : query.AsNoTracking<TEntity>();
    }

    public static IQueryable<T> ApplyIDentityOptions<T>(this IQueryable<T> query, RepositoryOptions<Tenant, string> options) where T : class
    {
        return options.UseAsTracking ? query.AsNoTrackingWithIdentityResolution<T>() : query.AsNoTrackingWithIdentityResolution<T>();
    }
}
