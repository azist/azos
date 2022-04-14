/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Collections;
using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Scripting.Dsl
{
  /// <summary>
  /// Custom steps get derived from this class
  /// </summary>
  public abstract class Step : INamed, IOrdered
  {
    public Step(StepRunner runner, IConfigSectionNode cfg, int order)
    {
      m_Runner = runner.NonNull(nameof(runner));
      m_Runner.App.InjectInto(this);
      m_Config = cfg.NonEmpty(nameof(cfg));
      m_Name = cfg.ValOf(Configuration.CONFIG_NAME_ATTR);
      if (m_Name.IsNullOrWhiteSpace())
      {
        if (this is EntryPoint) throw new RunnerException($"Missing a required declared name for an entry point at [{order}]");
        m_Name = Guid.NewGuid().ToString();
      }
      m_Order = order;

      ConfigAttribute.Apply(this, m_Config);
    }

    private readonly string m_Name;
    private readonly int m_Order;
    private readonly StepRunner m_Runner;
    private readonly IConfigSectionNode m_Config;

    public StepRunner         Runner => m_Runner;
    public IApplication       App    => Runner.App;
    public IConfigSectionNode Config => m_Config;

    /// <summary>
    /// Gives a mnemonic name for a step
    /// </summary>
    public string Name => m_Name;

    /// <summary>
    /// Step ordinal index in parent script, a step number is zero-based
    /// </summary>
    public int                Order  => m_Order;

    /// <summary>
    /// State is maintained between steps during a run instance as Step instances are not retained
    /// You can also use "Runner.GlobalState" for global storage
    /// </summary>
    public string Run(JsonDataMap state)
    {
      try
      {
        return DoRun(state);
      }
      catch(Exception error)
      {
        var handled = DoError(state, error);
        if (!handled) throw;
        return null;
      }
    }

    /// <summary>
    /// Override to perform work. Return NULL for next step or other step name to change control flow
    /// </summary>
    protected abstract string DoRun(JsonDataMap state);
    protected virtual bool DoError(JsonDataMap state, Exception error) => false;

    /// <summary>
    /// Writes a log message for this run step; returns the new log msg GDID for correlation, or GDID.Empty if no message was logged.
    /// The file/src are only used if `from` is null/blank
    /// </summary>
    protected internal virtual Guid WriteLog(Azos.Log.MessageType type,
                                               string from,
                                               string text,
                                               Exception error = null,
                                               Guid? related = null,
                                               string pars = null,
                                               [System.Runtime.CompilerServices.CallerFilePath]string file = null,
                                               [System.Runtime.CompilerServices.CallerLineNumber]int src = 0)
    {
      from = $"{this}.{from}";
      return Runner.WriteLog(type, from, text, error, related, pars, file, src);
    }

    public override string ToString() => $"{GetType().Name}(`{Name}`, [{Order}], '{Config.RootPath}')";

    /// <summary>
    /// A shortcut to <see cref="StepRunnerVarResolver.Eval(string, StepRunner, JsonDataMap)"/>
    /// </summary>
    public string Eval(string value, JsonDataMap state)
      => StepRunnerVarResolver.Eval(value, Runner, state);

  }


}
