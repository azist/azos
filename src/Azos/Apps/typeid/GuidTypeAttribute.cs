/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;

using Azos.Platform;

namespace Azos.Apps
{
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
      var result = TryGetGuidTypeAttribute<TDecorationTarget, TAttribute>(type);

      if (result == null)
        throw new AzosException(StringConsts.GUID_TYPE_RESOLVER_MISSING_ATTRIBUTE_ERROR.Args(type.FullName, typeof(TAttribute).FullName));

      return result;
    }

    /// <summary>
    /// Returns TAttribute:GuidTypeAttribute for a type.
    /// If type is not decorated by the attribute then null is returned
    /// </summary>
    public static TAttribute TryGetGuidTypeAttribute<TDecorationTarget, TAttribute>(Type type) where TAttribute : GuidTypeAttribute
    {
      var result = s_Cache[type.IsOfType<TDecorationTarget>(nameof(type))]?[typeof(TAttribute)] as TAttribute;
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
      TypeFguid = guid;
    }

    /// <summary>
    /// The Guid of the decorated type which can be mapped back to the Type object which it decorates.
    /// Logically this property stores the same ID as TypeFguid
    /// </summary>
    public readonly Guid TypeGuid;

    /// <summary>
    /// The Guid of the decorated type which can be mapped back to the Type object which it decorates.
    /// This representation stored guid as Fguid - a tuple of 2 ulongs for performance.
    /// Logically this property stores the same ID as TypeGuid
    /// </summary>
    public readonly Fguid TypeFguid;
  }


}
