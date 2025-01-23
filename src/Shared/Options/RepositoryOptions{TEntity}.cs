// Copyright (c) DefaultProjects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DefaultProjects.Shared.Interfaces;

namespace DefaultProjects.Shared.Options;

public record RepositoryOptions<TEntity> : RepositoryOptions<TEntity, string> where TEntity : class, IEntity<string> { }
