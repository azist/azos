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
  internal sealed class TypeSchema : ITypeSchema
  {
    public const string CONFIG_SCHEMA_SECTION = "schema";

    public const string CONFIG_ASSEMBLY_SECTION = "assembly";
    public const string CONFIG_FILE_ATTR = "file";
    public const string CONFIG_NS_PATTERN_ATTR = "ns-pattern";

    internal TypeSchema(IArea area, IConfigSectionNode cfg)
    {
      m_Area = area.NonNull(nameof(area));

      if (!cfg.NonEmpty(nameof(cfg)).IsSameName(CONFIG_SCHEMA_SECTION))
        cfg = cfg[CONFIG_SCHEMA_SECTION];

      cfg.NonEmpty(nameof(CONFIG_SCHEMA_SECTION));

      m_Assemblies = new Dictionary<string, BuildInformation>(StringComparer.OrdinalIgnoreCase);
      m_ObjectTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
      m_QueryTypes = new Dictionary<string, List<Type>>(StringComparer.OrdinalIgnoreCase);

      try
      {
        foreach(var node in cfg.ChildrenNamed(CONFIG_ASSEMBLY_SECTION))
        {
          var fn = node.ValOf(CONFIG_FILE_ATTR).NonBlank("{0}/${1}".Args(node.RootPath, CONFIG_FILE_ATTR));
          var asm = Assembly.LoadFrom(fn);
          var nsPat = node.ValOf(CONFIG_NS_PATTERN_ATTR).Default("*");
          load(asm, nsPat);
        }

        //Computer version digest on SORTED assembly Build infos
        //the operation is deferred after all assemblies finished loading
        m_Version = 0;
        foreach(var entry in m_Assemblies.OrderBy(e => e.Key)) //(deterministic order) based on Assembly FQN
        {
          m_Version ^= ShardKey.ForString(entry.Value.Content);//compute version digest hash based on the BuildInfo
        }
      }
      catch(Exception error)
      {
        throw new ConfigException("Bad config of {0}: {1}".Args(nameof(TypeSchema), error.ToMessageWithType()), error);
      }

    }

    private void load(Assembly asm, string nsPattern)
    {
      var asmName = asm.GetName().Name;
      if (!m_Assemblies.TryGetValue(asmName, out var bi))//if assembly is NOT in the set yet
      {
        //get BuildInfo for assembly
        bi = new BuildInformation(asm);
        //register entry
        m_Assemblies.Add(asmName, bi);
      }

      var all = asm.GetTypes();

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


      //Step I
      foreach(var t in otypes)//all HeapObject types
      {
        var atr = HeapAttribute.Lookup<HeapSpaceAttribute>(t);
        if (atr == null) continue;//safeguard

        if (!atr.Area.EqualsOrdIgnoreCase(m_Area.Name))
          throw new DataHeapException(StringConsts.DATA_HEAP_AREA_BINDING_ERROR.Args(t.DisplayNameWithExpandedGenericArgs(), m_Area.Name));

        if (m_ObjectTypes.TryGetValue(atr.Space, out var existing))
        {
          if (existing != t)
            throw new DataHeapException(StringConsts.DATA_HEAP_SPACE_BINDING_ERROR.Args(m_Area.Name,
                                                                                        atr.Space,
                                                                                        existing.DisplayNameWithExpandedGenericArgs(),
                                                                                        t.DisplayNameWithExpandedGenericArgs()));
          continue;
        }

        m_ObjectTypes.Add(atr.Space, t);
      }//foreach all heap Object types

      //Step II
      foreach(var t in qtypes)//all HeapQuery types
      {
        var atr = HeapAttribute.Lookup<HeapProcAttribute>(t);
        if (atr == null) continue;//safeguard

        if (!atr.Area.EqualsOrdIgnoreCase(m_Area.Name))
          throw new DataHeapException(StringConsts.DATA_HEAP_AREA_BINDING_ERROR.Args(t.DisplayNameWithExpandedGenericArgs(), m_Area.Name));

        if (!m_QueryTypes.TryGetValue(atr.Name, out var list))
        {
           list = new List<Type>();
           m_QueryTypes.Add(atr.Name, list);
        }

        if (!list.Contains(t)) list.Add(t);
      }//foreach all heap Query types
    }

    private IArea m_Area;

    private ulong m_Version;
    private Dictionary<string, BuildInformation> m_Assemblies;

    //maps space -> Type
    private Dictionary<string, Type> m_ObjectTypes;

    //maps proc_name -> Type
    private Dictionary<string, List<Type>> m_QueryTypes;


    ///<inheritdoc/>
    public IArea Area => m_Area;

    ///<inheritdoc/>
    public IEnumerable<KeyValuePair<string, BuildInformation>> Assemblies => m_Assemblies;

    ///<inheritdoc/>
    public ulong Version => m_Version;

    ///<inheritdoc/>
    public IEnumerable<Type> ObjectTypes => m_ObjectTypes.Values;

    ///<inheritdoc/>
    public IEnumerable<Type> QueryTypes => m_QueryTypes.Values.SelectMany(l => l);

    ///<inheritdoc/>
    public Type MapObjectSpace(string space) => m_ObjectTypes.TryGetValue(space.NonBlank(nameof(space)), out var t) ? t : throw $"Space `{space}`".IsNotFound();

    ///<inheritdoc/>
    public IEnumerable<Type> MapQueryProc(string proc) => m_QueryTypes.TryGetValue(proc.NonBlank(nameof(proc)), out var t) ? t : throw $"Proc `{proc}`".IsNotFound();

  }
}
