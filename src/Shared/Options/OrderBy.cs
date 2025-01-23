// Copyright (c) DefaultProjects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace DefaultProjects.Shared.Options;

using System.Linq.Expressions;

public record OrderBy<TEntity, TKey>(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> OrderByExpression, bool Descending = false);
