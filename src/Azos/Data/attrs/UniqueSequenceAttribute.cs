/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Azos.Data
{
  /// <summary>
  /// Provides information for unique sequence gen: scope and name
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class UniqueSequenceAttribute : Attribute
  {
    public UniqueSequenceAttribute(string scope)
    {
      Scope = scope;
    }

    public UniqueSequenceAttribute(string scope, string sequence)
    {
      Scope = scope;
      Sequence = sequence;
    }

    public UniqueSequenceAttribute(Type protoDoc)
    {
      if (protoDoc==null)
          throw new DataException(StringConsts.ARGUMENT_ERROR+ "{0}.ctor(protoDoc=null)".Args(GetType().Name));

      Prototype = GetForDocType(protoDoc);

      if (Prototype==null)
        throw new DataException(StringConsts.ARGUMENT_ERROR+ "{0}.ctor(protoDoc is not decorated by attr)".Args(GetType().Name));

      if (Prototype.Prototype!=null)
          throw new DataException(StringConsts.ARGUMENT_ERROR+ "{0}.ctor(protoDoc is pointing to another {0})".Args(GetType().Name));

      Scope = Prototype.Scope;
      Sequence = Prototype.Sequence;
    }

    public readonly UniqueSequenceAttribute Prototype;
    public readonly string Scope;
    public readonly string Sequence;


    private static volatile Dictionary<Type, UniqueSequenceAttribute> s_ScopeCache = new Dictionary<Type, UniqueSequenceAttribute>();

    /// <summary>
    /// Returns UniqueSequenceAttribute or null for doc type
    /// </summary>
    public static UniqueSequenceAttribute GetForDocType(Type tDoc)
    {
      if (tDoc == null || !typeof(Doc).IsAssignableFrom(tDoc))
        throw new DataException("UniqueSequenceAttribute.GetForDocType(tDoc isnt TypedDoc | null)");

      UniqueSequenceAttribute result;

      if (s_ScopeCache.TryGetValue(tDoc, out result)) return result;

      result = tDoc.GetCustomAttributes(typeof(UniqueSequenceAttribute), false)
                    .FirstOrDefault() as UniqueSequenceAttribute;

      var dict = new Dictionary<Type, UniqueSequenceAttribute>(s_ScopeCache);
      dict[tDoc] = result;
      System.Threading.Thread.MemoryBarrier();
      s_ScopeCache = dict; // atomic

      return result;
    }
  }
}
