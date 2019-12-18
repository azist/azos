using System;
using System.Collections.Generic;
using System.Text;

using Azos.Serialization.JSON;

namespace Azos.Data.AST
{
  /// <summary>
  /// Controls json serialization of Expression and its derivatives
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
