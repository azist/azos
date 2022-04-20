/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;
using Azos.Collections;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;
using Azos.Text;

namespace Azos.Scripting.Dsl
{
  /// <summary>
  /// Sets a global or local value to the specified expression
  /// </summary>
  /// <example><code>
  ///  do{ type="Set" global=a to='((x * global.a) / global.b) + 23'}
  ///  do{ type="Set" local=x to='x+1' name="inc x"}
  /// </code></example>
  public sealed class Set : Step
  {
    public const string CONFIG_TO_ATTR = "to";
    public const string CONFIG_GLOBAL_ATTR = "global";
    public const string CONFIG_LOCAL_ATTR = "local";

    public const string GLOBAL = "global";
    public const string LOCAL = "local";
    public const string UNKNOWN = "unknown";
    public const string RUNNER_RESULT = "runner.result";


    public Set(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx)
    {
      var toExpression = cfg.ValOf(CONFIG_TO_ATTR).NonBlank(CONFIG_TO_ATTR);
      m_Eval = new Evaluator(toExpression);

      m_Local = cfg.ValOf(CONFIG_LOCAL_ATTR);
      m_Global = cfg.ValOf(CONFIG_GLOBAL_ATTR);

      if (m_Local.IsNullOrWhiteSpace() && m_Global.IsNullOrWhiteSpace())
      {
        throw new RunnerException("Set step requires at least either global or local assignment");
      }
    }

    private Evaluator m_Eval;
    private string m_Global;
    private string m_Local;

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var got = m_Eval.Evaluate(id => StepRunnerVarResolver.GetResolver(Runner, id, state));

      if (m_Global.IsNotNullOrWhiteSpace())
      {
        Runner.GlobalState[m_Global] = got;
      }

      if (m_Local.IsNotNullOrWhiteSpace())
      {
        state[m_Local] = got;
      }

      return Task.FromResult<string>(null);
    }
  }

  /// <summary>
  /// Sets runner.Result
  /// </summary>
  /// <example><code>
  ///  do{ type="SetResult" to='((x * global.a) / global.b) + 23'}
  ///  do{ type="SetResult" to='x+1' name="inc x"}
  /// </code></example>
  public sealed class SetResult : Step
  {
    public SetResult(StepRunner runner, IConfigSectionNode cfg, int idx) : base(runner, cfg, idx)
    {
      var toExpression = cfg.ValOf(Set.CONFIG_TO_ATTR).NonBlank(Set.CONFIG_TO_ATTR);
      m_Eval = new Evaluator(toExpression);
    }

    private Evaluator m_Eval;

    protected override Task<string> DoRunAsync(JsonDataMap state)
    {
      var got = m_Eval.Evaluate(id => StepRunnerVarResolver.GetResolver(Runner, id, state));
      Runner.SetResult(got);
      return Task.FromResult<string>(null);
    }
  }

  /// <summary>
  /// Format:  Hello $(~x), my name is $(~global.y)!
  /// </summary>
  public sealed class StepRunnerVarResolver : IEnvironmentVariableResolver
  {
    /// <summary>
    /// Expands a format string of a form: "Hello {~global.name}, see you in {~local.x} minutes!"
    /// </summary>
    public static string FormatString(string fmt, StepRunner runner, JsonDataMap state)
    {
      try
      {
        if (fmt.IsNullOrWhiteSpace()) return fmt;
        return fmt.EvaluateVars(new StepRunnerVarResolver(runner, state), varStart: "{", varEnd: "}", recurse: false);
      }
      catch (Exception cause)
      {
        throw new RunnerException("{0} error while processing `{1}`: {2}".Args(nameof(GetResolver), fmt.TakeFirstChars(64, "..."), cause.ToMessageWithType()), cause);
      }
    }

    /// <summary>
    /// Evaluates a value that starts with a `~`. If a value does not start with `~` then keeps the value as-is.
    /// You can escape a literal value with double tilde like so: `~~a` -> `~a`
    /// </summary>
    public static string Eval(string value, StepRunner runner, JsonDataMap state)
    {
      try
      {
        if (value.IsNullOrWhiteSpace()) return value;
        if (value.StartsWith("~~")) return value.Substring(1);//skip escape
        if (value.StartsWith("~"))
        {
          var evaluator = new Evaluator(value.Substring(1));
          value = evaluator.Evaluate(id => GetResolver(runner, id, state));
        }
        return value;
      }
      catch (Exception cause)
      {
        throw new RunnerException("{0} error while processing `{1}`: {2}".Args(nameof(GetResolver), value.TakeFirstChars(64, "..."), cause.ToMessageWithType()), cause);
      }
    }



    /// <summary>
    /// Resolves `global.x` to `Runner.Globals[x]` otherwise to `state[x]`
    /// </summary>
    public static string GetResolver(StepRunner runner, string ident, JsonDataMap state)
    {
      try
      {
        return getResolver(runner, ident, state);
      }
      catch(Exception cause)
      {
        throw new RunnerException("{0} error on expression clause `{1}`: {2}".Args(nameof(GetResolver), ident.TakeFirstChars(64, "..."), cause.ToMessageWithType()), cause);
      }
    }

    private static string getResolver(StepRunner runner, string ident, JsonDataMap state)
    {
      string get(object v)
      {
        if (v == null) return string.Empty;
        if (v is string s) return s;

        var t = v.GetType();
        if (t.IsPrimitive) return v.AsString();

        var json = JsonWriter.Write(v, JsonWritingOptions.CompactRowsAsMap, System.Globalization.CultureInfo.InvariantCulture);
        return json;
      }

      string nav(IJsonDataObject obj, string path)
      {
        if (obj == null) return string.Empty;
        if (path.IsNullOrWhiteSpace()) return get(obj);

        var sp = path.SplitKVP('.');

        var elm = obj[sp.Key];
        if (sp.Value.IsNullOrWhiteSpace()) return get(elm);

        if (elm is IJsonDataObject d1) return nav(d1, sp.Value);

        if (elm is string str)
        {
          IJsonDataObject d2 = null;

          try
          {
            d2 = JsonReader.DeserializeDataObject(str);
          }
          catch
          {
            return string.Empty;//invalid path expression
          }

          return nav(d2, sp.Value);
        }

        return string.Empty;//invalid path expression
      }


      if (ident.IsNullOrWhiteSpace()) return ident;
      if (double.TryParse(ident, out var _)) return ident;

      if (ident.EqualsOrdSenseCase(Set.RUNNER_RESULT))
      {
        return get(runner.Result);
      }

      var pair = ident.SplitKVP('.');
      if (pair.Key == Set.GLOBAL)
        return nav(runner.GlobalState, pair.Value);
      else if (pair.Key == Set.LOCAL)
        return nav(state, pair.Value);
      else return ident;
    }


    public StepRunnerVarResolver(StepRunner runner, JsonDataMap state)
    {
      Runner = runner.NonNull(nameof(runner));
      State = state.NonNull(nameof(state));
    }

    public readonly StepRunner Runner;
    public readonly JsonDataMap State;

    public bool ResolveEnvironmentVariable(string name, out string value)
    {
      try
      {
        var eval = new Evaluator(name);
        value = eval.Evaluate(id => GetResolver(Runner, id, State));
        return true;
      }
      catch (Exception cause)
      {
        throw new RunnerException("{0} error while processing `{1}`: {2}".Args(nameof(GetResolver), name.TakeFirstChars(64, "..."), cause.ToMessageWithType()), cause);
      }
    }
  }


}
