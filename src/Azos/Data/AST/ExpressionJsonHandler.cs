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
    public override TypeCastResult TypeCastOnRead(object v, Type toType, bool fromUI, JsonReader.NameBinding nameBinding)
    {
      if (v is JsonDataMap map)
      {
        if (map.ContainsKey(nameof(ValueExpression.Value))) return new TypeCastResult(typeof(ValueExpression));
        if (map.ContainsKey(nameof(IdentifierExpression.Identifier))) return new TypeCastResult(typeof(IdentifierExpression));
        if (map.ContainsKey(nameof(UnaryExpression.Operand))) return new TypeCastResult(typeof(UnaryExpression));
        if (map.ContainsKey(nameof(BinaryExpression.LeftOperand))) return new TypeCastResult(typeof(BinaryExpression));
      }

      return TypeCastResult.NothingChanged;
    }
  }
}
