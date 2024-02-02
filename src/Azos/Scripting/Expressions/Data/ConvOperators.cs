/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Conf;
using Azos.Data;

namespace Azos.Scripting.Expressions.Data
{
  /// <summary>
  /// Performs value conversion using Convert.ChangeType api
  /// </summary>
  public class Cast : UnaryOperator<ScriptCtx, object, object>
  {
    [Config] public string TypeName{ get; set; }

    [Config] public string NullDefault { get; set; }

    public override object Evaluate(ScriptCtx context)
    {
      var val = Operand.NonNull(nameof(Operand)).Evaluate(context);
      var tp = Type.GetType(TypeName.NonBlank("Specified type name")).NonNull($"Existing type `{TypeName}`");

      if (val == null) return NullDefault.AsType(tp, false);

      var result = Convert.ChangeType(val, tp);
      return result;
    }
  }

  public class AsObject : UnaryOperator<ScriptCtx, object, object>
  {
    public override object Evaluate(ScriptCtx context) => Operand.NonNull(nameof(Operand)).Evaluate(context);
  }


  /// <summary>
  /// Returns constant value cast to the specified type (or string)
  /// </summary>
  public class Const : Expression<ScriptCtx, object>
  {
    [Config] public string TypeName { get; set; }

    [Config] public string Value { get; set; }

    public override object Evaluate(ScriptCtx context)
    {
      var tp = TypeName.IsNotNullOrEmpty() ? Type.GetType(TypeName).NonNull($"Existing type `{TypeName}`") : null;
      if (Value == null) return null;
      var result = tp == null ? Value : Value.AsType(tp, false);
      return result;
    }
  }

}
