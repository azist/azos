/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.Scripting.Dsl
{
  /// <summary>
  /// Designates states of script execution
  /// </summary>
  public enum RunStatus { Init = 0, Running, Finished, Crashed, Terminated  }

  /// <summary>
  /// Facilitates invocation of C# Steps from a script file in sequence.
  /// You can extend this class to supply extra use-case context-specific fields/props.
  /// This class is not thread-safe by design
  /// </summary>
  public class StepRunner
  {
    public const string CONFIG_STEP_SECTION = "do";
    public const double DEFAULT_TIMEOUT_SEC = 60.0d;

    public sealed class HaltSignal : Exception { }

    /// <summary>
    /// Represents a frame of call stack/flow. The shallow Owned collection gets deleted/deallocated on exit
    /// </summary>
    public sealed class Frame : DisposableObject
    {
      private static AsyncLocal<Frame> ats_Current = new AsyncLocal<Frame>();

      /// <summary>
      /// Returns a current frame in call chain or NULL if nothing was called
      /// </summary>
      public static Frame Current => ats_Current.Value;


      internal Frame(StepRunner runner)
      {
        Runner = runner;
        Owned = new List<object>();
        Caller = ats_Current.Value;
        ats_Current.Value = this;
      }
      protected override void Destructor()
      {
        ats_Current.Value = Caller;
        base.Destructor();
        Owned.ForEach(o =>
        {
          var v = o;
          if (v is IModuleImplementation mi) mi.ApplicationBeforeCleanup();
          DisposeIfDisposableAndNull(ref v);
        });
      }

      /// <summary> Runner that created this call frame </summary>
      public readonly StepRunner Runner;

      /// <summary> Call frame that called this one or null if this is the top-most frame </summary>
      public readonly Frame Caller;

      /// <summary> List of owned resource by THIS frame. These resources get disposed on frame scope dispose </summary>
      public readonly List<object> Owned;

      /// <summary>
      /// Returns all owned resources by all frames starting from the inner-most (entry point) frame first
      /// </summary>
      public IEnumerable<object> All => Caller?.All.Concat(Owned) ?? Owned;

      /// <summary>
      /// Returns a call stack, this frame is returned first
      /// </summary>
      public IEnumerable<Frame> Stack => Caller != null ? Caller.Stack.AddOneAtStart(this) : this.ToEnumerable();
    }


    public StepRunner(IApplication app, IConfigSectionNode rootSource, JsonDataMap globalState = null)
    {
      m_App = app.NonNull(nameof(app));
      m_RootSource = rootSource.NonEmpty(nameof(rootSource));
      m_GlobalState = globalState ?? new JsonDataMap(true);
      ConfigAttribute.Apply(this, m_RootSource);
    }

    private RunStatus m_Status = RunStatus.Init;
    private IApplication m_App;
    private JsonDataMap m_GlobalState;
    private object m_Result;
    private IConfigSectionNode m_RootSource;
    private Exception m_CrashError; protected void _SetCrashError(Exception ce) => m_CrashError = ce;

    /// <summary>
    /// Application context that this runner operates under
    /// </summary>
    public IApplication App => m_App;


    /// <summary>
    /// Root source of this script
    /// </summary>
    public IConfigSectionNode RootSource => m_RootSource;

    [Config]
    public virtual double TimeoutSec {  get; set; }


    /// <summary>
    /// Gets the runner execution result set by the <see cref="SetResult"/> method.
    /// This is typically called from steps which set the runner-global execution result
    /// which can later be captured into variables and used elsewhere
    /// </summary>
    public object Result => m_Result;

    /// <summary>
    /// Defines log Level
    /// </summary>
    [Config]
    public virtual Azos.Log.MessageType LogLevel {  get; set; }

    /// <summary>
    /// Runner global state, does not get reset between runs (unless you re-set it by step)
    /// </summary>
    public JsonDataMap GlobalState => m_GlobalState;

    /// <summary>
    /// Returns current run status
    /// </summary>
    public RunStatus Status => m_Status;

    /// <summary>
    /// True when Status is Running
    /// </summary>
    public bool IsRunning => m_Status == RunStatus.Running;


    /// <summary>
    /// Returns the last crash exception or null
    /// </summary>
    public Exception CrashError => m_CrashError;


    /// <summary>
    /// Sets the runner execution result accessible by the <see cref="Result"/> property.
    /// This is typically called from steps which set the runner-global execution result
    /// which can later be captured into variables and used elsewhere
    /// </summary>
    public void SetResult(object result) => m_Result = result;

    /// <summary>
    /// Executes the whole script. The <see cref="GlobalState"/> is NOT cleared automatically.
    /// Returns local state JsonDataMap (private to this run invocation)
    /// </summary>
    /// <param name="ep">EntryPoint instance</param>
    /// <param name="state">Local state</param>
    public async Task<JsonDataMap> RunAsync(EntryPoint ep, JsonDataMap state = null)
    {
      return await this.RunAsync(ep.NonNull(nameof(ep)).Name, state).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the whole script. The <see cref="GlobalState"/> is NOT cleared automatically.
    /// Returns local state JsonDataMap (private to this run invocation)
    /// </summary>
    /// <param name="state">Local state</param>
    /// <param name="entryPointStep">Name of step to start execution at, null by default - starts from the very first step</param>
    public async Task<JsonDataMap> RunAsync(string entryPointStep = null, JsonDataMap state = null)
    {
      Frame call = null;
      try
      {
        call = new Frame(this);
        return await DoRunAsync(entryPointStep, state).ConfigureAwait(false);
      }
      finally
      {
        call.Dispose();
      }
    }

    /// <summary>
    /// Executes the whole script. The <see cref="GlobalState"/> is NOT cleared automatically.
    /// Returns local state JsonDataMap (private to this run invocation)
    /// </summary>
    /// <param name="state">Local state</param>
    /// <param name="entryPointStep">Name of step to start execution at, null by default - starts from the very first step</param>
    protected virtual async Task<JsonDataMap> DoRunAsync(string entryPointStep = null, JsonDataMap state = null)
    {
      Exception error = null;
      try
      {
        m_Status = RunStatus.Running;

        if (state == null) state = new JsonDataMap(true);

        DoBeforeRun(state);

        OrderedRegistry<Step> script = new OrderedRegistry<Step>();

        Steps.ForEach(s => {

          var added = script.Register(s);

          if (!added) throw new RunnerException($"Duplicate runnable script step `{s.Name}` at '{s.Config.RootPath}'");
        });

        var time = Timeter.StartNew();
        var secTimeout = TimeoutSec;
        if (secTimeout <= 0.0) secTimeout = DEFAULT_TIMEOUT_SEC;

        var ip = 0;
        if (entryPointStep.IsNotNullOrWhiteSpace())
        {
          var ep = script[entryPointStep];
          if (ep==null) throw new RunnerException($"Entry point step `{entryPointStep}` was not found");
          if (!(ep is EntryPoint)) throw new RunnerException($"Entry point step `{entryPointStep}` is not of a valid EntryPoint type");
          ip = ep.Order;
        }

        while(ip < script.Count && m_Status == RunStatus.Running)
        {
          if (time.ElapsedSec > secTimeout)
          {
            throw new RunnerException("Timeout at {0} sec on [{1}]".Args(time.ElapsedSec, ip));
          }

          var step = script[ip];

          string nextStepName = null;
          try
          {
            //----------------------------
            nextStepName = await step.RunAsync(state).ConfigureAwait(false); //<----------- RUN
            //----------------------------
          }
          catch (Exception inner)
          {
            if (inner is HaltSignal) throw;
            throw new RunnerException($"Error on step {step}: {inner.ToMessageWithType()}", inner);
          }

          if (nextStepName.IsNullOrWhiteSpace())
          {
            ip++;
          }
          else
          {
            var next = script[nextStepName];
            if (next==null) throw new RunnerException($"Step not found: `{nextStepName}` by {step}");
            ip = next.Order;
          }
        }//for
      }
      catch(Exception cause)
      {
        if (!(cause is HaltSignal))
        {
          error = cause;
        }
      }

      var handled = false;
      m_CrashError = error;

      try
      {
        handled = DoAfterRun(error, state);
      }
      catch(Exception errorFromAfter)
      {
        m_Status = RunStatus.Crashed;
        if (error != null)
        {
          m_CrashError =  new AggregateException($"{nameof(DoAfterRun)} leaked:\n {error.ToMessageWithType()} \n and \n {errorFromAfter.ToMessageWithType()}", error, errorFromAfter);
          throw m_CrashError;
        }
        else
        {
          m_CrashError = errorFromAfter;
          throw;
        }
      }

      if (!handled && error != null)
      {
        m_Status = RunStatus.Crashed;
        throw error;
      }

      if (IsRunning) m_Status = RunStatus.Finished;

      return state;
    }

    /// <summary>
    /// If the status is Running, sets it to Terminated and returns true
    /// </summary>
    public bool Terminate()
    {
      var was = m_Status == RunStatus.Running;
      if (was) m_Status = RunStatus.Terminated;
      return was;
    }

    /// <summary>
    /// Invoked before steps, makes run state instance.
    /// Default implementation makes case-sensitive state bag
    /// </summary>
    protected virtual void DoBeforeRun(JsonDataMap state)
    {

    }

    /// <summary>
    /// Invoked after all steps are run, if error is present it is set and return true if
    /// you handle the error yourself, otherwise return false for default processing
    /// </summary>
    protected virtual bool DoAfterRun(Exception error, JsonDataMap state)
    {
      return false;
    }

    /// <summary>
    /// Returns all runnable steps, default implementation returns all sections named "STEP"
    /// in their declaration syntax
    /// </summary>
    public virtual IEnumerable<IConfigSectionNode> StepSections
      => m_RootSource.ChildrenNamed(CONFIG_STEP_SECTION);

    /// <summary>
    /// Returns materialized steps of <see cref="StepSections"/>
    /// </summary>
    public virtual IEnumerable<Step> Steps
    {
      get
      {
        var i = 0;
        foreach(var nstep in StepSections)
        {
          var step = FactoryUtils.Make<Step>(nstep, null, new object[] { this, nstep, i });
          yield return step;
          i++;
        }
      }
    }

    /// <summary>
    /// Returns explicit entry point names
    /// </summary>
    public IEnumerable<EntryPoint> EntryPoints => Steps.OfType<EntryPoint>();

    /// <summary>
    /// Writes a log message for this runner; returns the new log msg GDID for correlation, or GDID.Empty if no message was logged.
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
      if (type < LogLevel) return Guid.Empty;

      if (from.IsNullOrWhiteSpace())
        from = "{0}:{1}".Args(file.IsNotNullOrWhiteSpace() ? System.IO.Path.GetFileName(file) : "?", src);

      var msg = new Azos.Log.Message
      {
        App = App.AppId,
        Topic = CoreConsts.RUN_TOPIC,
        From = "{0}.{1}".Args(GetType().DisplayNameWithExpandedGenericArgs(), from),
        Type = type,
        Text = text,
        Exception = error,
        Parameters = pars,
        Source = src
      };

      msg.InitDefaultFields(App);

      if (related.HasValue) msg.RelatedTo = related.Value;

      App.Log.Write(msg);

      return msg.Guid;
    }

  }
}
