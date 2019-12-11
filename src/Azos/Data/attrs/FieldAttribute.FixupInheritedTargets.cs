/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Platform;
using Azos.Serialization.JSON;

namespace Azos.Data
{
  /// <summary>
  /// Provides information about table schema that this typed row is a part of
  /// </summary>
  public partial class FieldAttribute
  {
    /// <summary>
    /// Performs logical "inheritance" metadata expansion based on cloneTarget chain
    /// </summary>
    internal static void FixupInheritedTargets(string fieldName, IEnumerable<FieldAttribute> all)
    {
      if (all==null || !all.Any()) return;//safeguard

      var done = new HashSet<FieldAttribute>();
      var graph = new HashSet<FieldAttribute>();

      void process(FieldAttribute self)
      {
        if (!graph.Add(self)) throw new DataException("Cyclical target reference: "+self.TargetName);
        try
        {
          if (self.CloneFromTargetName == null)
          {
            done.Add(self);
            return;
          }

          var parent = all.Single( a => a.TargetName.EqualsOrdIgnoreCase(self.CloneFromTargetName));//this throws if none or > 1
          if (!done.Contains(parent))
          {
            process(parent);
          }
          //now merge DEP -> ONE
          inheritAttribute(parent, self);
          done.Add(self);
        }
        finally
        {
          graph.Remove(self);
        }
      }

      try
      {
        //in topological sort order using stack
        all.ForEach(a => process(a));
      }
      catch(Exception error)
      {
        throw new DataException("Fieldset `{0}` contains bad `cloneTarget` references to targets that are either not found in any [Field] attribute instance on that field, contain reference cycles, or missing the [Field] declarations without clone dependencies. The graph could not be resolved".Args(fieldName), error);
      }
    }

    private static void inheritAttribute(FieldAttribute parent, FieldAttribute self)
    {
      //merge attributes from parent into self prop by prop
      //....
    }

  }
}
