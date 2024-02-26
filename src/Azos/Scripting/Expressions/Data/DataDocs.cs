/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
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
    [Config] public bool Lax { get; set; }

    public sealed override object Evaluate(ScriptCtx context)
    {
      context.NonNull(nameof(context));
      var fns = Field.NonBlank(nameof(Field)).Split('.', StringSplitOptions.RemoveEmptyEntries);

      var ds = context.Data;
      object result = null;
      foreach(var fn in fns)
      {
        if (ds == null)
        {
          if (Lax) return null;
          throw new ScriptingException("Bad field by name navigation: `{0}`".Args(Field));
        }
        result = ds[fn.Replace("%2e", ".")];
        ds = result as Doc;
      }

      return result;
    }
  }


  /// <summary>
  /// Abstract accessor which enumerates fields by name. Multiple field must be separated by semicolon.
  /// Each field name property may have a single field or a path: "Patient.LastName"
  /// in which case "Patient" is a field of primary data context of type "Doc" having a field name "LastName".
  /// You can escape field names with dots with "%2e" for example: "Geometry.%2eX" = "Geometry[dot].X"
  /// </summary>
  public abstract class MultifieldByName<TResult> : Expression<ScriptCtx, TResult>
  {
    [Config]
    public string Fields { get; set; }

    [Config(Default = true)]
    public bool Lax { get; set; } = true;

    protected IEnumerable<(Doc doc, Schema.FieldDef fld, object val)> GetFields(ScriptCtx context)
    {
      context.NonNull(nameof(context));
      var paths = Fields.NonBlank(nameof(Fields)).Split(';');
      foreach(var path in paths.Where(p => p.IsNotNullOrWhiteSpace()))
      {
        var fns = path.NonBlank(nameof(path)).Split('.', StringSplitOptions.RemoveEmptyEntries);

        var ds = context.Data;
        Doc doc = ds;
        Schema.FieldDef fd = null;
        object fv = null;

        foreach (var fn in fns)
        {
          if (ds == null)
          {
            if (Lax) break;
            throw new ScriptingException("Bad field by name navigation: `{0}`".Args(path));
          }
          doc = ds;
          var dsfn = fn.Replace("%2e", ".");
          fd = ds.Schema[dsfn];
          if (fd == null)
            throw new ScriptingException("Bad field by name navigation: `{0}`. Unknown field: `{1}`".Args(path, dsfn));

          fv = ds.GetFieldValue(fd);
          ds = fv as Doc;
        }

        if (fd != null) yield return (doc, fd, fv);
      }//foreach
    }
  }

  /// <summary>
  /// Bool expression which is true if all fields are null
  /// </summary>
  public sealed class AllNull : MultifieldByName<bool>
  {
    public override bool Evaluate(ScriptCtx context)
     => GetFields(context).All(one => one.val == null);
  }

  /// <summary>
  /// Bool expression which is true if all fields are non-null
  /// </summary>
  public sealed class AllNonNull : MultifieldByName<bool>
  {
    public override bool Evaluate(ScriptCtx context)
      => GetFields(context).All(one => one.val != null);
  }

  /// <summary>
  /// Bool expression which is true if any of the fields are null
  /// </summary>
  public sealed class AnyNull : MultifieldByName<bool>
  {
    public sealed override bool Evaluate(ScriptCtx context)
      => GetFields(context).Any(one => one.val == null);
  }

  /// <summary>
  /// Bool expression which is true if any of the fields are not null
  /// </summary>
  public sealed class AnyNonNull : MultifieldByName<bool>
  {
    public sealed override bool Evaluate(ScriptCtx context)
      => GetFields(context).Any(one => one.val != null);
  }

  /// <summary>
  /// Returns the value of the first non-null field or null.
  /// You can optionally type cast the value to `CastTypeName` and
  /// apply optional `NullDefault`
  /// </summary>
  public sealed class FirstNonNull : MultifieldByName<object>
  {
    [Config] public string CastTypeName { get; set; }
    [Config] public string NullDefault { get; set; }

    public sealed override object Evaluate(ScriptCtx context)
    {
      var got = GetFields(context).FirstOrDefault(one => one.val != null).val;

      if (CastTypeName.IsNotNullOrWhiteSpace())
      {
        var tp = Type.GetType(CastTypeName).NonNull($"Existing type `{CastTypeName}`");
        if (got == null) return NullDefault.AsType(tp, false);
        return Convert.ChangeType(got, tp);
      }
      else
      {
        return got ?? NullDefault;
      }
    }
  }


}
