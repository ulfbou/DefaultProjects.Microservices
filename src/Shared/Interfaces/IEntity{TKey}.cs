// Copyright (c) DefaultProjects. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace DefaultProjects.Shared.Interfaces;

public interface IEntity<TKey> where TKey : notnull, IEquatable<TKey>
{
    TKey Id { get; }
    byte[] RowVersion { get; set; }
}
