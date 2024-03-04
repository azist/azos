/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
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
  /// You can escape field names with dots with "%2e" for example: "Geometry.%2eX" = "Geometry[dot].X".
  /// You can also use a different data doc root which should be set in the `ctx.State` object by its name: `dataset1:a.b.c`
  /// </summary>
  public class ByName : Expression<ScriptCtx, object>
  {
    [Config] public string Field { get; set; }
    [Config] public bool Lax { get; set; }

    public sealed override object Evaluate(ScriptCtx context)
    {
      var (_, _, result) = NavigateOneFieldPath(context.NonNull(nameof(context)), Field.NonBlank(nameof(Field)), Lax);
      return result;
    }

    /// <summary>
    /// As of CTX navigates a field nav expression of a form:  `state:f1.f2.fx` or `f1.f2.fx`. The state is a name of named dataset stores in `Ctx.State[name]`
    /// </summary>
    public static (Doc doc, Schema.FieldDef fld, object val) NavigateOneFieldPath(ScriptCtx ctx, string path, bool lax = false)// 20240304 DKh
    {
      path.NonBlank(nameof(path));

      Doc ds = ctx.Data;

      var (key, val) = path.SplitKVP(':');// key:a.b.c
      if (val.IsNotNullOrWhiteSpace())
      {
        path = val;
        ds = ctx.State[key].CastTo<Doc>($"State `{key}` is `Doc`");
      }

      var segs = path.Split('.', StringSplitOptions.RemoveEmptyEntries);

      Doc doc = ds;
      Schema.FieldDef fd = null;
      object fv = null;
      foreach (var seg in segs)
      {
        if (ds == null)
        {
          if (lax) break;
          throw new ScriptingException("Bad field by name navigation: `{0}`".Args(path));
        }

        //Check for []
        var (fn, sub) = seg.SplitKVP('[');
        if (sub.IsNotNullOrWhiteSpace())
        {
          if (sub[^1] != ']') throw new ScriptingException("Bad field by name navigation: `{0}`".Args(path));
          sub = sub[..^1];

        }
        if (fn.IsNullOrWhiteSpace()) throw new ScriptingException("Bad field by name navigation: `{0}`".Args(path));


        doc = ds;
        var dsfn = fn.Replace("%2e", ".")
                     .Replace("%5b", "[")
                     .Replace("%5d", "]")
                     .Replace("%3a", ":");

        fd = ds.Schema[dsfn];
        if (fd == null)
          throw new ScriptingException("Bad field by name navigation: `{0}`. Unknown field: `{1}`".Args(path, dsfn));

        fv = ds.GetFieldValue(fd);

        //Subscript check
        if (sub.IsNotNullOrWhiteSpace())
        {
          throw new ScriptingException("Field navigation subscripts are not supported yet: `{0}`".Args(path));
          //Interpret the subscript  fv as IEnumerable etc...
        }

        ds = fv as Doc;
      }//foreach seg
      return (doc, fd, fv);
    }
  }


  /// <summary>
  /// Abstract accessor which enumerates fields by name. Multiple field must be separated by semicolon.
  /// Each field name property may have a single field or a path: "Patient.LastName"
  /// in which case "Patient" is a field of primary data context of type "Doc" having a field name "LastName".
  /// You can escape field names with dots with "%2e" for example: "Geometry.%2eX" = "Geometry[dot].X"
  /// You can also use a different data doc root which should be set in the `ctx.State` object by its name: `dataset1:a.b.c`
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
        var escp = path.Replace("%3b", ";");
        var (doc, fd, fv) = ByName.NavigateOneFieldPath(context, escp, Lax);
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


  /// <summary>
  /// Abstraction for a bool function checking scalar values for equality
  /// </summary>
  public abstract class MultifieldScalarPredicate : MultifieldByName<bool>
  {
    protected bool Predicate(object v)
    {
      if (v == null)
      {
        if (Value == null) return true;
        return false;
      }
      var cv = Format.IsNullOrWhiteSpace() ? v.ToString() : Format.Args(v);
      return cv == Value;
    }

    [Config] public string Value { get; set; }
    [Config] public string Format { get; set; }
  }

  /// <summary>
  /// Bool predicate operator returning true WHEN ALL values of the referenced fields are equal to
  /// the specified scalar value
  /// </summary>
  public abstract class AllScalarsAre : MultifieldScalarPredicate
  {
    public override bool Evaluate(ScriptCtx context) => GetFields(context).All(one => Predicate(one.val));
  }

  /// <summary>
  /// Bool predicate operator returning true WHEN ALL values of the referenced fields are NOT equal to
  /// the specified scalar value
  /// </summary>
  public abstract class AllScalarsAreNot : MultifieldScalarPredicate
  {
    public override bool Evaluate(ScriptCtx context) => GetFields(context).All(one => !Predicate(one.val));
  }

  /// <summary>
  /// Bool predicate operator returning true WHEN ANY values of the referenced fields are equal to
  /// the specified scalar value
  /// </summary>
  public abstract class AnyScalarsAre : MultifieldScalarPredicate
  {
    public override bool Evaluate(ScriptCtx context) => GetFields(context).Any(one => Predicate(one.val));
  }

  /// <summary>
  /// Bool predicate operator returning true WHEN ANY values of the referenced fields are NOT equal to
  /// the specified scalar value
  /// </summary>
  public abstract class AnyScalarsAreNot : MultifieldScalarPredicate
  {
    public override bool Evaluate(ScriptCtx context) => GetFields(context).Any(one => !Predicate(one.val));
  }




}
