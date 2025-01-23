// Copyright (c) DefaultProjects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace DefaultProjects.Shared.Options;

using DefaultProjects.Shared.Interfaces;

using System.Linq.Expressions;

public record RepositoryOptions<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : notnull, IEquatable<TKey>
{
    public const int DefaultTake = 100;
    public const int DefaultSkip = 0;

    public static RepositoryOptions<TEntity, TKey> Default { get; } = new();

    public bool UseAsTracking { get; init; }
    public IEnumerable<Expression<Func<TEntity, object>>> NavigationProperties { get; init; }
    public IEnumerable<OrderBy<TEntity, TKey>> OrderBy { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
    public bool DisableTracking { get; init; }
    public Expression<Func<TEntity, bool>>? Filter { get; init; }

    public RepositoryOptions(
        bool? useAsTracking = null,
        IEnumerable<Expression<Func<TEntity, object>>>? navigationProperties = null,
        IEnumerable<OrderBy<TEntity, TKey>>? orderBy = null,
        int? skip = null,
        int? take = null,
        bool? disableTracking = null,
        Expression<Func<TEntity, bool>>? filter = null)
    {
        UseAsTracking = useAsTracking ?? false;
        NavigationProperties = navigationProperties ?? [];
        OrderBy = orderBy ?? [];
        Skip = skip ?? DefaultSkip;
        Take = take ?? DefaultTake;
        DisableTracking = disableTracking ?? false;
        Filter = filter;
    }
}
