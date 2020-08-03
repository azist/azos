/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


using Azos.Conf;
using Azos.Platform;
using Azos.Serialization.JSON;

namespace Azos.Data
{
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
          if (self.DeriveFromTargetName == null)
          {
            done.Add(self);
            return;
          }

          var parent = all.Single( a => a.TargetName.EqualsSenseCase(self.DeriveFromTargetName));//this throws if none or > 1
          if (!done.Contains(parent))
          {
            process(parent);
          }
          //now merge DEP -> ONE
          inheritAttribute(parent, self, fieldName);
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
        throw new DataException(StringConsts.CRUD_FIELDDEF_TARGET_DERIVATION_ERROR.Args(fieldName), error);
      }
    }

    private static readonly IEnumerable<PropertyInfo> ALL_PROPS =
                               typeof(FieldAttribute).GetProperties(BindingFlags.Public |
                                                                    BindingFlags.Instance)
                                                     .Where(pi => pi.Name != nameof(TargetName) && pi.CanWrite && pi.SetMethod.IsPublic);

    private static void inheritAttribute(FieldAttribute parent, FieldAttribute self, string callSite)
    {
      //merge attributes from parent into self prop by prop
      foreach(var pi in ALL_PROPS)
      {
        if (pi.Name==nameof(MetadataContent))
        {
          if (self.MetadataContent.IsNullOrWhiteSpace())
            self.MetadataContent = parent.MetadataContent;
          else if (parent.MetadataContent.IsNotNullOrWhiteSpace())
          { //merge
            var conf1 = ParseMetadataContent(parent.MetadataContent, callSite);
            var conf2 = ParseMetadataContent(self.MetadataContent, callSite);

            var merged = new LaconicConfiguration();
            merged.CreateFromMerge(conf1, conf2);
            self.MetadataContent = merged.SaveToString();
          }

          continue;
        }//metadata merge

        if (pi.Name==nameof(ValueList))
        {
          if (self.ValueList==null && self.PropertyWasAssigned(nameof(ValueList)))//explicit reset
          {
            self.ValueList = null;
            continue;
          }
          else if (!self.HasValueList)
            self.ValueList = parent.ValueList;
          else if (parent.HasValueList)
          { //merge
            var vl1 = parent.ParseValueList(true);
            var vl2 = self.ParseValueList(true);

            vl2.Append(vl1);//merge missing in self from parent



            //remove all that start with REMOVE
            // to remove a key include an override with:  `keyABC: #del#` (item keyed on `keyABC` will be removed)
            const string DELETE = "#del#";
            vl2.Where(kvp => kvp.Value.AsString().EqualsOrdIgnoreCase(DELETE))
               .ToArray()
               .ForEach( kvp => vl2.Remove(kvp.Key) );

            self.ValueList = BuildValueListString(vl2);//reconstitute jsonmap back into string
          }

          continue;
        }

        if (self.PropertyWasAssigned(pi.Name)) continue;//was overridden
        pi.SetValue(self, pi.GetValue(parent));//set value
      }
    }

  }
}
