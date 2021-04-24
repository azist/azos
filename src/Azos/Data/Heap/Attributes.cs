/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Base for heap-related attributes
  /// </summary>
  public abstract class HeapAttribute : Attribute
  {
    private static Platform.FiniteSetLookup<Type, HeapAttribute> s_Cache =
       new Platform.FiniteSetLookup<Type, HeapAttribute>(t => t.GetCustomAttribute<HeapAttribute>(false));

    /// <summary>
    /// Lookup a subtype of HeapAttribute on a type in an efficient way. Throws if such attribute decoration is not declared
    /// </summary>
    public static T Lookup<T>(Type t) where T : HeapAttribute
    {
      var attr =  s_Cache[t.NonNull(nameof(t))];
      var result = attr as T;
      if (result == null)
      {
        var site = $"{nameof(HeapAttribute)}.{nameof(Lookup)}";
        var td = typeof(T).DisplayNameWithExpandedGenericArgs();
        throw new CallGuardException(site,
                                     td,
                                     "{0}: `{1}` is not decorated with `[{2}]`".Args(site, t.DisplayNameWithExpandedGenericArgs(), td));
      }
      return result;
    }


    protected HeapAttribute(string area)
    {
      Area = area.CheckId(nameof(area));
    }

    /// <summary>
    /// Area Id. The value has to comply with Heap Id requirement: ASCII-only number or digit with either dash or underscore separators.
    /// The Ids are case-insensitive but their case is preserved as-is
    /// </summary>
    public string Area { get; private set; }
  }


  /// <summary>
  /// Assigns (AREA, SPACE) tuple to HeapObjects. The attribute must be set to bind
  /// a CLI type to space/collection within data heap area.
  /// You can only bind no more than one CLR object type to a named collection space at a time.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class HeapSpaceAttribute : HeapAttribute
  {
    public HeapSpaceAttribute(string area, string space) : base(area)
    {
      Space = space.CheckId(nameof(space));
    }

    /// <summary>
    /// Name of data space (feature space) bound to the decorated HeapObject type.
    /// The value has to comply with Heap Id requirement: ASCII-only number or digit with either dash or underscore separators.
    /// The Ids are case-insensitive but their case is preserved as-is
    /// </summary>
    public string Space{ get; private set; }

    //public Atom Channel { get; private set; }

    //public string ChannelName
    //{
    //  get => Channel.Value;
    //  set => Channel = Atom.Encode(value.NonBlank(nameof(ChannelName)));
    //}

  }

  /// <summary>
  /// Assigns (AREA, Name) tuple to heap procedure. The attribute must be set to bind
  /// a CLI type to procedure by name within the data heap area.
  /// You can only bind no more than one heap procedure to query type
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class HeapProcAttribute : HeapAttribute
  {
    public HeapProcAttribute(string area, string name) : base(area)
    {
      Name = name.NonBlank(nameof(name));
    }

    /// <summary>
    /// Name of heap procedure/handler on the server.
    /// The name is not case-sensitive within heap
    /// </summary>
    public string Name { get; private set; }
  }
}
