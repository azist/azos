/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;

namespace Azos.Conf.Forest.Server
{
  /// <summary>
  /// A tuple of (Atom idForest, Atom idTree)
  /// </summary>
  public struct TreePtr : IEquatable<TreePtr>
  {
    public TreePtr(Atom idForest, Atom idTree)
    {
      IdForest = idForest;
      IdTree = idTree;
    }

    public TreePtr(EntityId id)
    {
      IdForest = id.System;
      IdTree = id.Type;
    }

    public readonly Atom IdForest;
    public readonly Atom IdTree;

    public bool IsAssigned => !IdForest.IsZero;

    public bool Equals(TreePtr other) => this.IdForest == other.IdForest && this.IdTree == other.IdTree;

    public override int GetHashCode() => IdForest.GetHashCode() ^ IdTree.GetHashCode();
    public override bool Equals(object obj) => obj is TreePtr other ? this.Equals(other) : false;

    public override string ToString() => $"(`{IdForest}`, `{IdTree}`)";

    public static bool operator ==(TreePtr a, TreePtr b) => a.Equals(b);
    public static bool operator !=(TreePtr a, TreePtr b) => !a.Equals(b);
  }
}
