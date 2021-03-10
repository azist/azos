/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Serialization.JSON;

namespace Azos.Data.AST
{
  /// <summary>
  /// Controls Json polymorphic serialization of Expression and its derivatives
  /// </summary>
  public sealed class ExpressionJsonHandlerAttribute : JsonHandlerAttribute
  {
    private static readonly Dictionary<string, Type> PATTERNS = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase)
    {
      {nameof(ValueExpression.Value),           typeof(ValueExpression)},
      {nameof(ArrayValueExpression.ArrayValue), typeof(ArrayValueExpression)},
      {nameof(IdentifierExpression.Identifier), typeof(IdentifierExpression)},
      {nameof(UnaryExpression.Operand),         typeof(UnaryExpression)},
      {nameof(BinaryExpression.LeftOperand),    typeof(BinaryExpression)},
      {nameof(BinaryExpression.RightOperand),   typeof(BinaryExpression)}
    };

    public override TypeCastResult TypeCastOnRead(object v, Type toType, bool fromUI, JsonReader.DocReadOptions options)
    {
      if (v is JsonDataMap map)
      {
        foreach(var k in map.Keys)
         if (PATTERNS.TryGetValue(k, out var type)) return new TypeCastResult(type);
      }

      return TypeCastResult.NothingChanged;
    }
  }
}
