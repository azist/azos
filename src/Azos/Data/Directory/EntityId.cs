using System;

using Azos.Data.Access;

namespace Azos.Data.Directory
{
  /// <summary>
  /// Identifies a directory entity by a unique id within a type
  /// </summary>
  public struct EntityId : IEquatable<EntityId>, IDistributedStableHashProvider
  {
    /// <summary>
    /// Entity type. Type names are case-insensitive
    /// </summary>
    public readonly string Type;

    /// <summary>
    /// A unique Id of an entity. Ids are case sensitive
    /// </summary>
    public readonly string Id;

    public override int GetHashCode() => Id==null ? 0 : Id.GetHashCode();

    public override bool Equals(object obj)
    => obj is EntityId eid ? this.Equals(eid) : false;

    public bool Equals(EntityId other)
    => this.Type.EqualsIgnoreCase(other.Type) &&
       this.Id.EqualsOrdSenseCase(other.Id);

    public ulong GetDistributedStableHash() => ShardingUtils.StringToShardingID(Id);

    public static bool operator ==(EntityId a, EntityId b) => a.Equals(b);
    public static bool operator !=(EntityId a, EntityId b) => !a.Equals(b);
  }
}
