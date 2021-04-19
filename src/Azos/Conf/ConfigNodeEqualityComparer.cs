/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

namespace Azos.Conf
{
  /// <summary>
  /// Performs deep structural comparison of IConfigNodes
  /// </summary>
  public sealed class ConfigNodeEqualityComparer : EqualityComparer<IConfigNode>
  {
    private static ConfigNodeEqualityComparer s_Instance = new ConfigNodeEqualityComparer();

    public static ConfigNodeEqualityComparer Instance => s_Instance;

    private ConfigNodeEqualityComparer() { }

    public override bool Equals(IConfigNode x, IConfigNode y)
    {
      if (x == null && y == null) return true;
      if (x == null) return false;
      if (y == null) return false;
      if (x.Exists != y.Exists) return false;
      if (!x.Exists) return true;

      if (!x.Name.EqualsOrdIgnoreCase(y.Name)) return false;

      if (!x.VerbatimValue.EqualsOrdSenseCase(y.VerbatimValue)) return false;

      var snodex = x as IConfigSectionNode;
      if (snodex != null)
      {
        var snodey = y as IConfigSectionNode;
        if (snodey == null) return false;

        if (snodex.ChildCount != snodey.ChildCount) return false;
        if (snodex.AttrCount != snodey.AttrCount) return false;

        for (var i = 0; i < snodex.ChildCount; i++)
        {
          var xn = snodex[i];
          var yn = snodey[i];
          if (!this.Equals(xn, yn)) return false;
        }

        for (var i = 0; i < snodex.AttrCount; i++)
        {
          var xn = snodex.AttrByIndex(i);
          var yn = snodey.AttrByIndex(i);
          if (!this.Equals(xn, yn)) return false;
        }
      }
      else if (y is IConfigSectionNode) return false;

      return true;
    }

    public override int GetHashCode(IConfigNode node)
    {
      if (node == null || !node.Exists) return 0;

      var hc = node.Name.GetHashCodeOrdIgnoreCase();
      if (node.VerbatimValue != null)
        hc ^= node.VerbatimValue.GetHashCodeOrdSenseCase();

      var snode = node as IConfigSectionNode;
      if (snode != null)
      {
        foreach (var c in snode.Children)
          hc ^= this.GetHashCode(c);

        foreach (var a in snode.Attributes)
          hc ^= this.GetHashCode(a);
      }

      return hc;
    }
  }
}
