// Copyright (c) DefaultProjects.Microservices. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace DefaultProjects.Shared.Interfaces;

public interface ITenantEntity<TKey> : IEntity<TKey>
        where TKey : notnull, IEquatable<TKey>
{
    string TenantId { get; set; }
}
