/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Provides "[BI]t-e[X]change" aka "BIX" serialization services for DAG of typed data document
  /// objects which are classes deriving from TypedDoc. Bix is a schema-change/version tolerant
  /// serialization mechanism supporting data doc polymorphism, arrays and lists.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Bix is a binary format conceptually somewhat similar to Protobuf, however it capitalizes on
  /// unified framework thus it does not require serialization-specific metadata/IDL as it uses
  /// existing Azos data documents as-is. The field bindings are done by name where field names are Atoms
  /// (up to 8 char ASCII monikers) represented as var bit encoded ULONGs, consequently short names take a few bytes.
  /// </para>
  /// <para>
  /// Bix utilizes IAmorphousData concept when it can not match the type/name on read, calling "AfterLoad" hook at the end
  /// of deserialization, this way it is possible to transparently upgrade existing/historical data to newer formats
  /// </para>
  /// <para>
  /// Bix performs polymorphic processing of TypeDoc derivatives but does not preserve the references, that is:
  /// if for example a root doc has two field pointing to the same child doc instance, then those would be serialized
  /// twice (and deserialized) as two independent objects. This design decision is done for simplicity and performance as
  /// reference preservation is rarely needed in practice. Should you need a normalized object graph with logical references
  /// preserved,consider shaping your datagram accordingly (e.g. use array indexes as pointers)
  /// </para>
  /// <para>
  /// While it is possible to inter-operate between other formats, Bix is built for high performance,
  /// features and bare-bones maintenance and implementation simplicity, as such its code base is orders
  /// of magnitude smaller than that of Protobuf and other similar frameworks.
  /// </para>
  /// </remarks>
  public static class Bixer
  {
    /// <summary>
    /// Declares target name used by Bix serialization framework for Before/After call hooks on forms etc..
    /// </summary>
    public const string BIXER_TARGET = "~~~BIXER~~~";

    private static object s_Lock = new object();
    private static volatile Dictionary<Type, BixCore> s_Cores = new Dictionary<Type, BixCore>();


    /// <summary>
    /// Registers all IBixCore implementors from a satellite assembly of the calling assembly.
    /// A satellite assembly is the one that is called the same name as the calling one with
    /// an additional suffix appended at the end. If no specific suffix is passed, then ".Bixer" is used as default.
    /// A satellite assembly typically has its file co-located on disk with the specified assembly
    /// </summary>
    /// <example> Given "MyBusiness.dll" satellite is "MyBusiness.Bixer.dll" </example>
    public static void RegisterTypeSerializationSatelliteForThisAssembly(string suffix = null)
    {
      var thisAssembly = Assembly.GetCallingAssembly();
      RegisterTypeSerializationSatelliteFor(thisAssembly);
    }

    /// <summary>
    /// Registers all IBixCore implementors from a satellite assembly of the specified assembly.
    /// A satellite assembly is the one that is called the same name as the specified one with
    /// an additional suffix appended at the end. If no specific suffix is passed, then ".Bixer" is used as default.
    /// A satellite assembly typically has its file co-located on disk with the specified assembly
    /// </summary>
    /// <example> Given "MyBusiness.dll" satellite is "MyBusiness.Bixer.dll" </example>
    public static void RegisterTypeSerializationSatelliteFor(Assembly assembly, string suffix = null)
    {
      const string EXT = ".dll";
      const string BIXER_ASSEMBLY_SUFFIX = ".Bixer";

      var asmFileName = assembly.NonNull(nameof(assembly)).Location.Trim();
      var i = asmFileName.LastIndexOf(EXT);
      if (i != asmFileName.Length - EXT.Length)
        throw new BixException(StringConsts.AROW_SATELLITE_ASSEMBLY_NAME_ERROR.Args(asmFileName));

      asmFileName = asmFileName.Substring(0, i) + suffix.Default(BIXER_ASSEMBLY_SUFFIX);

      try
      {
        var asm = Assembly.LoadFrom(asmFileName);
        RegisterTypeSerializationCores(asm);
      }
      catch (Exception error)
      {
        throw new BixException(StringConsts.AROW_SATELLITE_ASSEMBLY_LOAD_ERROR.Args(asmFileName, error.ToMessageWithType()), error);
      }
    }

    /// <summary>
    /// Registers all IBixCore type serialization handlers from the specified assembly so
    /// so they can be globally recognized for serialization of TypedDoc in Bix format
    /// </summary>
    public static void RegisterTypeSerializationCores(Assembly asm)
    {
      var allTypes = asm.NonNull(nameof(asm)).GetTypes();
      var allCores = allTypes.Where(t => t.IsClass && !t.IsAbstract && typeof(BixCore).IsAssignableFrom(t));

      lock(s_Lock)
      {
        var dict = new Dictionary<Type, BixCore>(s_Cores);

        allCores.ForEach( tc => {
          var core = Activator.CreateInstance(tc) as BixCore;
          var targetType = core.GetTargetType();
          dict[targetType] = core;
        });

        System.Threading.Thread.MemoryBarrier();

        s_Cores = dict;//atomic
      }
    }

    /// <summary>
    /// Registers IBixCore so it can be globally recognized for serialization of TypedDoc in Bix format
    /// </summary>
    public static bool RegisterTypeSerializationCore(BixCore core)
    {
      var targetType = core.NonNull(nameof(core)).GetTargetType();

      lock (s_Lock)
      {
        if (s_Cores.TryGetValue(targetType, out var existing) && object.ReferenceEquals(existing, core)) return false;
        var dict = new Dictionary<Type, BixCore>(s_Cores);
        dict[targetType] = core;

        System.Threading.Thread.MemoryBarrier();

        s_Cores = dict;//atomic

        return true;
      }
    }

  }
}
