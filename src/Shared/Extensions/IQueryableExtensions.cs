using Microsoft.EntityFrameworkCore;
using DefaultProjects.Shared.Options;

namespace DefaultProjects.Shared.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int page, int pageSize)
    {
        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }

    // Apply RepositoryOptions to query
    public static IQueryable<T> ApplyOptions<T>(this IQueryable<T> query, RepositoryOptions options) where T : class
    {
        return options.UseAsTracking ? query.AsTracking<T>() : query.AsNoTracking<T>();
    }

    public static IQueryable<T> ApplyIDentityOptions<T>(this IQueryable<T> query, RepositoryOptions options) where T : class
    {
        return options.UseAsTracking ? query.AsNoTrackingWithIdentityResolution<T>() : query.AsNoTrackingWithIdentityResolution<T>();
    }
}
