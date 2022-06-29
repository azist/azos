/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Data.Heap
{
  /// <summary>
  /// References an object instance on a data heap. It is a tuple of (ShardKey, GDID).
  /// Each distinct data heap object (an entity) has one and only one distinct/unique reference <see cref="GDID"/> Id.
  /// The Id is what defines object identity (a primary key) per object type.
  /// The ShardKey provides shard/partition routing information to locate objects.
  /// The concept is similar to `(void*)` in C-like languages
  /// </summary>
  public struct ObjectRef : IEquatable<ObjectRef>
  {
    public ObjectRef(GDID id)
    {
      Shard = new ShardKey(id);
      Id = id;
    }

    public ObjectRef(ShardKey shard, GDID id)
    {
      Shard = shard;
      Id = id;
    }

    /// <summary>
    ///  The `ShardKey` provides shard/partition routing information to locate objects
    /// </summary>
    public readonly ShardKey Shard;

    /// <summary>
    /// The `Id` is what defines object identity (a primary key) per object type.
    /// </summary>
    public readonly GDID Id;

    /// <summary>
    /// True when the structure state is assigned - when the `Id` is not `GDID.ZERO` which is a
    /// logical equivalent of pointer `NULL`
    /// </summary>
    public bool Assigned => !Id.IsZero;

    public override bool Equals(object obj) => obj is ObjectRef ptr ? this.Equals(ptr) : false;
    public bool Equals(ObjectRef other) => this.Id == other.Id && this.Shard == other.Shard;
    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() => $"Ref(`{Shard}` -> `{Id}`)";

    public static bool operator ==(ObjectRef a, ObjectRef b) => a.Equals(b);
    public static bool operator !=(ObjectRef a, ObjectRef b) => !a.Equals(b);
  }
}
