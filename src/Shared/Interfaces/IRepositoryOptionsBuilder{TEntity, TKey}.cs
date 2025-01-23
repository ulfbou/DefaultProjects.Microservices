using DefaultProjects.Shared.Options;

using System.Linq.Expressions;

namespace DefaultProjects.Shared.Interfaces;

public interface IRepositoryOptionsBuilder<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TKey : notnull, IEquatable<TKey>
{
    RepositoryOptions<TEntity, TKey> Build();
    IRepositoryOptionsBuilder<TEntity, TKey> WithAsTracking(bool useAsTracking = true);
    IRepositoryOptionsBuilder<TEntity, TKey> WithDisableTracking(bool disableTracking = true);
    IRepositoryOptionsBuilder<TEntity, TKey> WithNavigation<TProperty>(Expression<Func<TEntity, TProperty>> navigation);
    IRepositoryOptionsBuilder<TEntity, TKey> WithOrderBy(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByExpression);
    IRepositoryOptionsBuilder<TEntity, TKey> WithOrderByDescending(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByExpression);
    IRepositoryOptionsBuilder<TEntity, TKey> WithSkip(int skip);
    IRepositoryOptionsBuilder<TEntity, TKey> WithTake(int take);
    IRepositoryOptionsBuilder<TEntity, TKey> WithFilter(Expression<Func<TEntity, bool>> filter);
}
