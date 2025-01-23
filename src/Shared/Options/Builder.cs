// Copyright (c) DefaultProjects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace DefaultProjects.Shared.Options;

using DefaultProjects.Shared.Interfaces;

using FluentInjections.Validation;

using System.Linq.Expressions;

public class Builder
{
    public static IRepositoryOptionsBuilder<TEntity, TKey> For<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        return new RepositoryOptionsBuilder<TEntity, TKey>();
    }

    internal class RepositoryOptionsBuilder<TEntity, TKey> : IRepositoryOptionsBuilder<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
        where TKey : notnull, IEquatable<TKey>
    {
        private bool _useAsTracking;
        private readonly List<Expression<Func<TEntity, object>>> _navigationProperties = new();
        private readonly List<OrderBy<TEntity, TKey>> _orderBy = new();
        private int? _skip;
        private int? _take;
        private bool _disableTracking;
        private Expression<Func<TEntity, bool>>? _filter;

        public IRepositoryOptionsBuilder<TEntity, TKey> WithAsTracking(bool useAsTracking = true)
        {
            _useAsTracking = useAsTracking;
            return this;
        }

        public IRepositoryOptionsBuilder<TEntity, TKey> WithDisableTracking(bool disableTracking = true)
        {
            _disableTracking = disableTracking;
            return this;
        }

        public IRepositoryOptionsBuilder<TEntity, TKey> WithNavigation<TProperty>(Expression<Func<TEntity, TProperty>> navigation)
        {
            _navigationProperties.Add((navigation as Expression<Func<TEntity, object>>)!);
            return this;
        }

        public IRepositoryOptionsBuilder<TEntity, TKey> WithOrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByExpression)
        {
            _orderBy.Add(new OrderBy<TEntity, TKey>(orderByExpression, false));
            return this;
        }

        public IRepositoryOptionsBuilder<TEntity, TKey> WithOrderByDescending(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByExpression)
        {
            _orderBy.Add(new OrderBy<TEntity, TKey>(orderByExpression, true));
            return this;
        }

        public IRepositoryOptionsBuilder<TEntity, TKey> WithSkip(int skip)
        {
            Guard.NotNegative(skip, nameof(skip));
            _skip = skip;
            return this;
        }

        public IRepositoryOptionsBuilder<TEntity, TKey> WithTake(int take)
        {
            Guard.NotNegative(take, nameof(take));
            _take = take;
            return this;
        }

        public IRepositoryOptionsBuilder<TEntity, TKey> WithFilter(Expression<Func<TEntity, bool>> filter)
        {
            _filter = filter;
            return this;
        }

        public RepositoryOptions<TEntity, TKey> Build()
        {
            return new RepositoryOptions<TEntity, TKey>(
                _useAsTracking,
                _navigationProperties.AsReadOnly(),
                _orderBy.AsReadOnly(),
                _skip,
                _take,
                _disableTracking,
                _filter);
        }
    }
}
