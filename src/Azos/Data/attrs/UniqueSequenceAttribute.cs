/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Reflection;

using Azos.Platform;

namespace Azos.Data
{
  /// <summary>
  /// Provides information for unique sequence gen: scope and name.
  /// This is used for GDID and other ID generation.
  /// Warning: DO NOT base your scope and sequence names on CLR entity type names (such as class names)
  /// because they may change with refactoring, instead use this attribute to bind to immutable
  /// scope and sequence name for ID generation
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class UniqueSequenceAttribute : Attribute
  {
    /// <summary>
    /// Provides information for unique sequence gen: scope and name.
    /// This is used for GDID and other ID generation.
    /// Warning: DO NOT base your scope and sequence names on CLR entity type names (such as class names)
    /// because they may change with refactoring, instead use this attribute to bind to immutable
    /// scope and sequence name for ID generation
    /// </summary>
    public UniqueSequenceAttribute(string scope, string sequence)
    {
      Scope = scope.NonBlank(nameof(scope));
      Sequence = sequence.NonBlank(nameof(sequence));
    }

    /// <summary>
    /// Inherits attribute definition from another data document type.
    /// The call may not be chained
    /// </summary>
    public UniqueSequenceAttribute(Type protoDoc)
    {
      if (protoDoc == null)
          throw new DataException(StringConsts.ARGUMENT_ERROR+ "{0}.ctor(protoDoc=null)".Args(GetType().Name));

      Prototype = GetForDocType(protoDoc);

      if (Prototype == null)
        throw new DataException(StringConsts.ARGUMENT_ERROR+ "{0}.ctor(protoDoc is not decorated by attr)".Args(GetType().Name));

      if (Prototype.Prototype != null)
          throw new DataException(StringConsts.ARGUMENT_ERROR+ "{0}.ctor(protoDoc is pointing to another {0})".Args(GetType().Name));

      Scope = Prototype.Scope;
      Sequence = Prototype.Sequence;
    }

    public readonly UniqueSequenceAttribute Prototype;

    /// <summary>
    /// Scope name for unique sequence generation, such as GDID
    /// </summary>
    public readonly string Scope;

    /// <summary>
    /// Sequence name for unique id generation, such as GDID
    /// </summary>
    public readonly string Sequence;


    private static FiniteSetLookup<Type, UniqueSequenceAttribute> s_Cache = new FiniteSetLookup<Type, UniqueSequenceAttribute>
    (
      t => t.IsOfType<Doc>(nameof(t)).GetCustomAttribute<UniqueSequenceAttribute>(false)
    );

    /// <summary>
    /// Returns UniqueSequenceAttribute or null for doc type
    /// </summary>
    public static UniqueSequenceAttribute GetForDocType(Type tDoc) => s_Cache[tDoc];
  }


  /// <summary>
  /// Provides extensions for use with UniqueSequenceAttribute
  /// </summary>
  public static class UniqueSequenceAttributeExtensions
  {
    /// <summary>
    /// Generates one GDID from the supplied provider taking its scope and sequence name from <see cref="UniqueSequenceAttribute"/>
    /// </summary>
    /// <param name="provider">Provider to use</param>
    /// <param name="tDoc">Type of data Doc</param>
    /// <returns>One GDID</returns>
    public static GDID GenerateGdidFor(this Idgen.IGdidProvider provider, Type tDoc)
    {
      provider.NonNull(nameof(provider));
      tDoc.IsOfType<Doc>(nameof(tDoc));

      var atr = UniqueSequenceAttribute.GetForDocType(tDoc);

      atr.NonNull($"`{tDoc.Name}` decorated with `{nameof(UniqueSequenceAttribute)}`");

      var result = provider.GenerateOneGdid(atr.Scope, atr.Sequence);
      return result;
    }

    /// <summary>
    /// Tries to generate many consecutive Globally-Unique distributed ID (GDID) from the same authority for the supplied sequence name.
    /// If the reserved block gets exhausted, then the returned ID array length may be less than requested
    /// The scope and sequence name are taken from <see cref="UniqueSequenceAttribute"/>
    /// </summary>
    /// <param name="provider">Provider to use</param>
    /// <param name="tDoc">Type of data Doc</param>
    /// <param name="gdidCount">How many GDIDS to generate</param>
    /// <returns>The GDID[] instance which may have less elements than requested by gdidCount</returns>
    public static GDID[] TryGenerateManyConsecutiveGdidsFor(this Idgen.IGdidProvider provider, Type tDoc, int gdidCount)
    {
      provider.NonNull(nameof(provider));
      tDoc.IsOfType<Doc>(nameof(tDoc));
      gdidCount.IsTrue( v => v > 0, "gdidCount > 0");

      var atr = UniqueSequenceAttribute.GetForDocType(tDoc);

      atr.NonNull($"`{tDoc.Name}` decorated with `{nameof(UniqueSequenceAttribute)}`");

      var result = provider.TryGenerateManyConsecutiveGdids(atr.Scope, atr.Sequence, gdidCount);
      return result;
    }
  }

}
