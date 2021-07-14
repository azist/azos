/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Specifies a logical route for event delivery, a tuple of (Namespace, Queue, ShardKey)
  /// </summary>
  public struct Route : IEquatable<Route>
  {
    public Route(Atom ns, Atom queue, ShardKey partition)
    {
      Namespace = ns.IsTrue( v => !v.IsZero && v.IsValid, "ns atom");
      Queue = queue.IsTrue(v => !v.IsZero && v.IsValid, "queue atom");
      Partition = partition;//partition may be unassigned
    }

    public readonly Atom Namespace;
    public readonly Atom Queue;
    public readonly ShardKey Partition;

    /// <summary>
    /// True if the structure represents an assigned value
    /// </summary>
    public bool Assigned => !Namespace.IsZero;

    public bool Equals(Route other)
      => this.Namespace == other.Namespace &&
         this.Queue     == other.Queue &&
         this.Partition == other.Partition;

    public override int GetHashCode()
      => Namespace.GetHashCode() ^
         Queue    .GetHashCode() ^
         Partition.GetHashCode();

    public override bool Equals(object obj)
      => obj is Route other ? this.Equals(other) : false;

    public override string ToString() => $"{Namespace}::{Queue}[{Partition}]";

    public static bool operator ==(Route a, Route b) => a.Equals(b);
    public static bool operator !=(Route a, Route b) => !a.Equals(b);
  }
}
