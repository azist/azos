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

namespace Azos.Apps
{
  /// <summary>
  /// Provides default type resolver implementation which looks for types in the listed assemblies.
  /// The searched types must be of TDecorationTarget descent decorated with the specified TAttribute attribute
  /// </summary>
  public class GuidTypeResolver<TDecorationTarget, TAttribute> : IGuidTypeResolver where TDecorationTarget: class where TAttribute : GuidTypeAttribute
  {
    public const string CONFIG_ASSEMBLY_SECTION = "assembly";
    public const string CONFIG_NS_ATTR = "ns";


    internal GuidTypeResolver()
    {
      m_Cache1 = new Dictionary<Guid, Type>();
      m_Cache2 = new Dictionary<Fguid, Type>();
      System.Threading.Thread.MemoryBarrier();
    }

    public GuidTypeResolver(params Type[] types)
    {
       if (types==null || types.Length==0) throw new AzosException(StringConsts.ARGUMENT_ERROR + "GuidTypeResolver.ctor(types==null|Empty)");

       var mappings = types.ToDictionary( t => GuidTypeAttribute.GetGuidTypeAttribute<TDecorationTarget, TAttribute>(t).TypeGuid, t => t );
       ctor(mappings);
    }

    public GuidTypeResolver(IDictionary<Guid, Type> mappings)
    {
      if (mappings==null || !mappings.Any()) throw new AzosException(StringConsts.ARGUMENT_ERROR + "GuidTypeResolver.ctor(mappings==null|Empty)");
      ctor(mappings);
    }

    private void ctor(IDictionary<Guid, Type> mappings)
    {
      if (mappings.Count != mappings.Distinct().Count())
        throw new AzosException(StringConsts.ARGUMENT_ERROR + "GuidTypeResolver.ctor(mappings has duplicates)");

      m_Cache1 = new Dictionary<Guid, Type>(mappings);
      m_Cache2 = new Dictionary<Fguid, Type>();
      m_Cache1.ForEach(kvp => m_Cache2.Add(kvp.Key, kvp.Value));

      System.Threading.Thread.MemoryBarrier();
    }

    public GuidTypeResolver(IConfigSectionNode conf)
    {
      m_Cache1 = new Dictionary<Guid, Type>();
      m_Cache2 = new Dictionary<Fguid, Type>();

      if (conf == null || !conf.Exists) return;

      foreach (var nasm in conf.Children.Where(n => n.IsSameName(CONFIG_ASSEMBLY_SECTION)))
      {
        var asmName = nasm.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
        var asmNS = nasm.AttrByName(CONFIG_NS_ATTR).Value;
        if (asmName.IsNullOrWhiteSpace()) continue;

        var asm = Assembly.LoadFrom(asmName);

        foreach (var type in asm.GetTypes().Where( t => t.IsClass && !t.IsAbstract && typeof(TDecorationTarget).IsAssignableFrom(t))) //.Where(t => t.IsPublic && t.IsClass && !t.IsAbstract && typeof(TDecorationTarget).IsAssignableFrom(t)))
        {
          if (asmNS.IsNotNullOrWhiteSpace() && !Azos.Text.Utils.MatchPattern(type.FullName, asmNS)) continue;
          var atr = GuidTypeAttribute.TryGetGuidTypeAttribute<TDecorationTarget, TAttribute>(type);
          if (atr==null) continue;

          Type existing;
          if (m_Cache1.TryGetValue(atr.TypeGuid, out existing))
            throw new AzosException(StringConsts.GUID_TYPE_RESOLVER_DUPLICATE_ATTRIBUTE_ERROR.Args(type.FullName, atr.TypeGuid, existing.FullName));

          m_Cache1.Add(atr.TypeGuid, type);
        }
      }

      if (m_Cache1.Count == 0)
        throw new AzosException(StringConsts.GUID_TYPE_RESOLVER_NO_TYPES_ERROR.Args(this.GetType().DisplayNameWithExpandedGenericArgs()));

      m_Cache1.ForEach(kvp => m_Cache2.Add(kvp.Key, kvp.Value));

      System.Threading.Thread.MemoryBarrier();
    }

    private volatile Dictionary<Guid, Type> m_Cache1;
    private volatile Dictionary<Fguid, Type> m_Cache2;


    public void AddTypes(IEnumerable<(Guid id, Type type)> batch, bool throwDups = true)
    {
      if (batch==null) return;
      if (!batch.Any() || batch.All(e => m_Cache1.TryGetValue(e.id, out var _))) return;

      var cache1 = new Dictionary<Guid, Type>(m_Cache1);
      var cache2 = new Dictionary<Fguid, Type>(m_Cache2);
      foreach(var pair in batch)
      {
        if (throwDups && cache1.TryGetValue(pair.id, out var existing) && pair.type != existing)
        {
          throw new AzosException(StringConsts.GUID_TYPE_RESOLVER_DUPLICATE_ATTRIBUTE_ERROR.Args(pair.type.FullName, pair.id, existing.FullName));
        }

        cache1[pair.id] = pair.type;
        cache2[pair.id] = pair.type;
      }
      System.Threading.Thread.MemoryBarrier();
      m_Cache1 = cache1;
      m_Cache2 = cache2;
      System.Threading.Thread.MemoryBarrier();
    }


    /// <summary>
    /// Resolves the GUID into type object or return null
    /// </summary>
    public Type TryResolve(Guid guid)
    {
      if (m_Cache1.TryGetValue(guid, out var result)) return result;
      return null;
    }

    /// <summary>
    /// Resolves the GUID into type object or throws
    /// </summary>
    public Type Resolve(Guid guid)
    {
      if (m_Cache1.TryGetValue(guid, out var result)) return result;
      throw new AzosException(StringConsts.GUID_TYPE_RESOLVER_ERROR.Args(guid, typeof(TDecorationTarget).Name));
    }

    /// <summary>
    /// Resolves the GUID into type object or return null
    /// </summary>
    public Type TryResolve(Fguid guid)
    {
      if (m_Cache2.TryGetValue(guid, out var result)) return result;
      return null;
    }

    /// <summary>
    /// Resolves the GUID into type object or throws
    /// </summary>
    public Type Resolve(Fguid guid)
    {
      if (m_Cache2.TryGetValue(guid, out var result)) return result;
      throw new AzosException(StringConsts.GUID_TYPE_RESOLVER_ERROR.Args(guid, typeof(TDecorationTarget).Name));
    }
  }
}
