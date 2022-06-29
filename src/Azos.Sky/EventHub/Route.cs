/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Sky.EventHub
{
  /// <summary>
  /// Specifies a logical route for event delivery, a tuple of (Namespace, Queue)
  /// </summary>
  public struct Route : IEquatable<Route>
  {
    /// <summary>
    /// Initializes logical route for event delivery, a tuple of (Namespace, Queue)
    /// </summary>
    public Route(Atom ns, Atom queue)
    {
      Namespace = ns.IsTrue( v => !v.IsZero && v.IsValid, "ns atom");
      Queue = queue.IsTrue(v => !v.IsZero && v.IsValid, "queue atom");
    }

    /// <summary> Namespace - a logical catalog containing the named queue within the network </summary>
    /// <remarks>Analog of db instance name on a server</remarks>
    public readonly Atom Namespace;

    /// <summary> Queue name within the namespace which is within the specified network </summary>
    /// <remarks>Analog of db table/collection</remarks>
    public readonly Atom Queue;

    /// <summary>
    /// True if the structure represents an assigned value
    /// </summary>
    public bool Assigned => !Namespace.IsZero;

    public bool Equals(Route other) => this.Namespace == other.Namespace && this.Queue == other.Queue;

    public override int GetHashCode() => Namespace.GetHashCode() ^ Queue.GetHashCode();

    public override bool Equals(object obj) => obj is Route other ? this.Equals(other) : false;

    public override string ToString() => $"{Namespace}.{Queue}";

    public static bool operator ==(Route a, Route b) => a.Equals(b);
    public static bool operator !=(Route a, Route b) => !a.Equals(b);
  }
}
