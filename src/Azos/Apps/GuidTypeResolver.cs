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
  /// Describe an entity that resolve type Guid ids into CLR type objects
  /// </summary>
  public interface IGuidTypeResolver
  {
    /// <summary>
    /// Tries to resolves the GUID into type or returns null
    /// </summary>
    Type TryResolve(Guid guid);


    /// <summary>
    /// Resolves the GUID into type or throws
    /// </summary>
    Type Resolve(Guid guid);
  }

  /// <summary>
  /// Provides information about the decorated type: assigns a globally-unique immutable type id
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public abstract class GuidTypeAttribute : Attribute
  {
    /// <summary>
    /// Returns TAttribute:GuidTypeAttribute for a type represented by a Guid.
    /// If type is not decorated by the attribute then exception is thrown
    /// </summary>
    public static TAttribute GetGuidTypeAttribute<TDecorationTarget, TAttribute>(Guid guid, IGuidTypeResolver resolver) where TAttribute : GuidTypeAttribute
    {
      var type = resolver.NonNull(nameof(resolver)).Resolve(guid);
      return GetGuidTypeAttribute<TDecorationTarget, TAttribute>(type);
    }

    /// <summary>
    /// Returns TAttribute:GuidTypeAttribute for a type.
    /// If type is not decorated by the attribute then exception is thrown
    /// </summary>
    public static TAttribute GetGuidTypeAttribute<TDecorationTarget, TAttribute>(Type type) where TAttribute : GuidTypeAttribute
    {
      var result = s_Cache[type.IsOfType<TDecorationTarget>(nameof(type))]?[typeof(TAttribute)] as TAttribute;

      if (result == null)
        throw new AzosException(StringConsts.GUID_TYPE_RESOLVER_MISSING_ATTRIBUTE_ERROR.Args(type.FullName, typeof(TAttribute).FullName));

      return result;
    }

    private static ConstrainedSetLookup<Type, ConstrainedSetLookup<Type, GuidTypeAttribute>> s_Cache =
      new ConstrainedSetLookup<Type, ConstrainedSetLookup<Type, GuidTypeAttribute>>(
        ttarget => new ConstrainedSetLookup<Type, GuidTypeAttribute>( tattr => ttarget.GetCustomAttribute(tattr, false) as GuidTypeAttribute)
    );


    protected GuidTypeAttribute(string typeGuid)
    {
      if (!Guid.TryParse(typeGuid.NonBlank(nameof(typeGuid)), out var guid))
        throw new AzosException(StringConsts.ARGUMENT_ERROR + GetType().FullName + ".ctor(typeGuid unparsable)");

      TypeGuid = guid;
    }

    /// <summary>
    /// The Guid of the decorated type which can be mapped back to the Type object which it decorates
    /// </summary>
    public readonly Guid TypeGuid;
  }

  /// <summary>
  /// Provides default type resolver implementation which looks for types in the listed assemblies.
  /// The searched types must be of TDecorationTarget descent decorated with the specified TAttribute attribute
  /// </summary>
  public class GuidTypeResolver<TDecorationTarget, TAttribute> : IGuidTypeResolver where TDecorationTarget: class where TAttribute : GuidTypeAttribute
  {
    public const string CONFIG_ASSEMBLY_SECTION = "assembly";
    public const string CONFIG_NS_ATTR = "ns";


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

      m_Cache = new Dictionary<Guid, Type>(mappings);

      System.Threading.Thread.MemoryBarrier();
    }

    public GuidTypeResolver(IConfigSectionNode conf)
    {
      m_Cache = new Dictionary<Guid, Type>();

      if (conf == null || !conf.Exists) return;

      foreach (var nasm in conf.Children.Where(n => n.IsSameName(CONFIG_ASSEMBLY_SECTION)))
      {
        var asmName = nasm.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
        var asmNS = nasm.AttrByName(CONFIG_NS_ATTR).Value;
        if (asmName.IsNullOrWhiteSpace()) continue;

        var asm = Assembly.LoadFrom(asmName);

        foreach (var type in asm.GetTypes().Where(t => t.IsPublic && t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(TDecorationTarget))))
        {
          if (asmNS.IsNotNullOrWhiteSpace() && !Azos.Text.Utils.MatchPattern(type.FullName, asmNS)) continue;
          var atr = GuidTypeAttribute.GetGuidTypeAttribute<TDecorationTarget, TAttribute>(type);

          Type existing;
          if (m_Cache.TryGetValue(atr.TypeGuid, out existing))
            throw new AzosException(StringConsts.GUID_TYPE_RESOLVER_DUPLICATE_ATTRIBUTE_ERROR.Args(type.FullName, atr.TypeGuid, existing.FullName));

          m_Cache.Add(atr.TypeGuid, type);
        }
      }

      if (m_Cache.Count == 0)
        throw new AzosException(StringConsts.GUID_TYPE_RESOLVER_NO_TYPES_ERROR.Args(this.GetType().DisplayNameWithExpandedGenericArgs()));

      System.Threading.Thread.MemoryBarrier();
    }

    private Dictionary<Guid, Type> m_Cache;


    /// <summary>
    /// Resolves the GUID into type object or return null
    /// </summary>
    public Type TryResolve(Guid guid)
    {
      if (m_Cache.TryGetValue(guid, out var result)) return result;
      return null;
    }

    /// <summary>
    /// Resolves the GUID into type object or throws
    /// </summary>
    public Type Resolve(Guid guid)
    {
      if (m_Cache.TryGetValue(guid, out var result)) return result;
      throw new AzosException(StringConsts.GUID_TYPE_RESOLVER_ERROR.Args(guid, typeof(TDecorationTarget).Name));
    }
  }
}
