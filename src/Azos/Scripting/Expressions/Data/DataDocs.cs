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
  /// Gets primary document: Context.Data property
  /// </summary>
  public class CtxData : Expression<ScriptCtx, object>
  {
    public sealed override object Evaluate(ScriptCtx context) => context?.Data;
  }

  /// <summary>
  /// Gets field value by name from context primary data. The field name property may include a path for example: "Patient.LastName"
  /// in which case "Patient" is a field of primary data context of type "Doc" having a field name "LastName".
  /// You can escape field names with dots with "%2e" for example: "Geometry.%2eX" = "Geometry[dot].X"
  /// </summary>
  public class ByName : Expression<ScriptCtx, object>
  {
    [Config] public string Field { get; set; }

    public sealed override object Evaluate(ScriptCtx context)
    {
      context.NonNull(nameof(context));
      var fns = Field.NonBlank(nameof(Field)).Split('.', StringSplitOptions.RemoveEmptyEntries);

      var ds = context.Data;
      object result = null;
      foreach(var fn in fns)
      {
        if (ds == null) throw new ScriptingException("Bad field by name navigation: `{0}`".Args(Field));
        result = ds[fn.Replace("%2e", ".")];
        ds = result as Doc;
      }

      return result;
    }
  }

}
