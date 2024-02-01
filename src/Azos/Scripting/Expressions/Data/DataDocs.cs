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
  /// Implements a document field by name accessor operator.
  /// Field value is read from an operand evaluated to a `Doc`
  /// </summary>
  public class ByNameOfDoc : UnaryOperator<ScriptCtx, object, object>
  {
    [Config] public string Field{  get; set; }

    public sealed override object Evaluate(ScriptCtx context)
    {
      var operand = Operand.NonNull(nameof(Operand));
      var fn = Field.NonBlank(nameof(Field));

      var doc = operand.Evaluate(context).CastTo<IDataDoc>("Operand is Doc");
      return doc[fn];
    }
  }

  /// <summary>
  /// Gets field value by name from context primary data
  /// </summary>
  public class ByName : Expression<ScriptCtx, object>
  {
    [Config] public string Field { get; set; }

    public sealed override object Evaluate(ScriptCtx context)
    {
      var fn = Field.NonBlank(nameof(Field));

      return context?.Data?[fn];
    }
  }

}
