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
using Azos.Text;

namespace Azos.Data.Heap.Implementation
{
  /// <summary>
  /// Type registry which mounts and checks types: HeapObjects and HeapQueries
  /// </summary>
  internal class TypeSchema
  {
    public const string CONFIG_SCHEMA_SECTION = "schema";

    public const string CONFIG_ASSEMBLY_SECTION = "assembly";
    public const string CONFIG_FILE_ATTR = "file";
    public const string CONFIG_NS_PATTERN_ATTR = "ns-pattern";

    public TypeSchema(IArea area, IConfigSectionNode cfg)
    {
      m_Area = area.NonNull(nameof(area));

      if (!cfg.NonEmpty(nameof(cfg)).IsSameName(CONFIG_SCHEMA_SECTION))
        cfg = cfg[CONFIG_SCHEMA_SECTION];

      cfg.NonEmpty(nameof(CONFIG_SCHEMA_SECTION));

      m_ObjectTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
      m_QueryTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

      try
      {
        foreach(var node in cfg.ChildrenNamed(CONFIG_ASSEMBLY_SECTION))
        {
          var fn = node.ValOf(CONFIG_FILE_ATTR).NonBlank("{0}/${1}".Args(node.RootPath, CONFIG_FILE_ATTR));
          var asm = Assembly.LoadFrom(fn);
          var nsPat = node.ValOf(CONFIG_NS_PATTERN_ATTR).Default("*");
          load(asm, nsPat);
        }
      }
      catch(Exception error)
      {
        throw new ConfigException("Bad config of {0}: {1}".Args(nameof(TypeSchema), error.ToMessageWithType()), error);
      }

    }

    private void load(Assembly asm, string nsPattern)
    {
      var all = asm.GetExportedTypes();

      var otypes = all.Where(t => t.IsPublic &&
                                  !t.IsAbstract &&
                                  typeof(HeapObject).IsAssignableFrom(t) &&
                                  Attribute.IsDefined(t, typeof(HeapSpaceAttribute), false) &&
                                  t.Namespace.MatchPattern(nsPattern, senseCase: true));

      var qtypes = all.Where(t => t.IsPublic &&
                                  !t.IsAbstract &&
                                  typeof(HeapQuery).IsAssignableFrom(t) &&
                                  Attribute.IsDefined(t, typeof(HeapProcAttribute), false) &&
                                  t.Namespace.MatchPattern(nsPattern, senseCase: true));



      foreach(var t in otypes)//all HeapObject types
      {
        var atr = HeapAttribute.Lookup<HeapSpaceAttribute>(t);
        if (atr == null) continue;//safeguard

        if (!atr.Area.EqualsOrdIgnoreCase(m_Area.Name))
          throw new DataHeapException(StringConsts.DATA_HEAP_AREA_BINDING_ERROR.Args(t.DisplayNameWithExpandedGenericArgs(), m_Area.Name));

        if (m_ObjectTypes.TryGetValue(atr.Space, out var existing))
          throw new DataHeapException(StringConsts.DATA_HEAP_SPACE_BINDING_ERROR.Args(m_Area.Name,
                                                                                      atr.Space,
                                                                                      existing.DisplayNameWithExpandedGenericArgs(),
                                                                                      t.DisplayNameWithExpandedGenericArgs()));

        m_ObjectTypes.Add(atr.Space, t);
      }//foreach all heap Object types

      //foreach(var t in qtypes)
      //  queries.Add(t);

    }

    private IArea m_Area;

    //maps space -> Type
    private Dictionary<string, Type> m_ObjectTypes;

    //maps proc_name -> Type
    private Dictionary<string, Type> m_QueryTypes;

    public IEnumerable<Type> ObjectTypes => m_ObjectTypes.Values;
    public IEnumerable<Type> QueryTypes => m_QueryTypes.Values;

  }
}
