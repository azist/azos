/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Performs handling of polymorphic Json TypedDoc data documents using BIX type code discriminator.
  /// Add this attribute to fields of type descendant of TypedDoc and arrays or list of those types,
  /// to handle polymorphic Json deserialization
  /// </summary>
  public sealed class BixJsonHandler : JsonHandlerAttribute
  {
    public const string BIX_DISCRIMINATOR = "__bix";

    /// <summary>
    /// Conditionally emits a BIX_DISCRIMINATOR field to the json map if the instance type is decorated with Bix attribute.
    /// Returns true when discriminator field was added
    /// </summary>
    public static bool EmitJsonBixDiscriminator(TypedDoc self, IDictionary<string, object> map)
    {
      if (self==null || map==null) return false;
      var t = self.GetType();
      var atr = BixAttribute.TryGetGuidTypeAttribute<TypedDoc, BixAttribute>(t);
      if (atr==null) return false;
      map[BIX_DISCRIMINATOR] = atr.TypeGuid;
      return true;
    }

    public override TypeCastResult TypeCastOnRead(object v, Type toType, bool fromUI, JsonReader.DocReadOptions options)
    {
      if (v is JsonDataMap map)
      {
        var tid = map[BIX_DISCRIMINATOR].AsGUID(Guid.Empty);
        if (tid==Guid.Empty) return TypeCastResult.NothingChanged;

        map.Remove(BIX_DISCRIMINATOR);

        var tp = Bixer.GuidTypeResolver.TryResolve(tid);

        if (tp==null) return TypeCastResult.NothingChanged;

        return new TypeCastResult(tp);
      }

      return TypeCastResult.NothingChanged;
    }
  }
}
