/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Apps.Injection;
using Azos.Apps;
using Azos.Conf;
using Azos.Data;

namespace Azos.Sky.Workers
{
  /// <summary>
  /// Represents a context for units of work (like Todos) executing in the distributed system.
  /// Processes are controlled via SkySystem.ProcessManager implementation
  /// </summary>
  [Serializable]
  public abstract class Process : AmorphousTypedDoc
  {

    [Inject] ISkyApplication m_App;

    public ISkyApplication App => m_App.NonNull(nameof(m_App));

    /// <summary>
    /// Factory method that creates new Process based on provided PID
    /// </summary>
    public static TProcess MakeNew<TProcess>(IApplication app, PID pid) where TProcess : Process, new()
      => makeDefault(app.AsSky(), new TProcess(), pid);

    /// <summary>
    /// Factory method that creates new Process based on provided Type, PID and Configuration
    /// </summary>
    public static Process MakeNew(IApplication app, Type type, PID pid, IConfigSectionNode args)
     => makeDefault(app.AsSky(), FactoryUtils.MakeAndConfigure<Process>(args, type), pid);

    private static TProcess makeDefault<TProcess>(ISkyApplication app, TProcess process, PID pid) where TProcess : Process
    {

      app.DependencyInjector.InjectInto(process);

      var attr = GuidTypeAttribute.GetGuidTypeAttribute<Process, ProcessAttribute>(process.GetType());

      var descriptor = new ProcessDescriptor(
        pid,
        attr.Description,
        app.TimeSource.UTCNow,
        "{0}@{1}@{2}".Args(Ambient.CurrentCallUser.Name, app.Name, app.HostName));

      process.m_SysDescriptor = descriptor;
      return process;
    }

    protected Process() { }

    private ProcessDescriptor m_SysDescriptor;

    /// <summary>
    /// Infrastructure method, developers do not call
    /// </summary>
    public void ____Deserialize(ProcessDescriptor descriptor)
    { m_SysDescriptor = descriptor; }

    /// <summary>
    /// Globally-unique ID of the TODO
    /// </summary>
    public PID SysPID { get { return m_SysDescriptor.PID; } }

    /// <summary>
    /// When was created
    /// </summary>
    public ProcessDescriptor SysDescriptor { get { return m_SysDescriptor; } }

    public override string ToString() { return "{0}('{1}')".Args(GetType().FullName, SysPID); }

    public override int GetHashCode() { return m_SysDescriptor.GetHashCode(); }

    public override bool Equals(Doc other)
    {
      var otherProcess = other as Process;
      if (otherProcess==null) return false;
      return this.m_SysDescriptor.PID.ID == otherProcess.m_SysDescriptor.PID.ID;
    }

    /// <summary>
    /// Executes merge operation on the existing process and another instance which tries to get spawned.
    /// This method MUST execute be VERY FAST and only contain merge logic, do not make external IO calls -
    /// all business data must already be contained in the original and another instance
    /// </summary>
    protected internal abstract void Merge(IProcessHost host, DateTime utcNow, Process another);

    protected internal virtual ResultSignal Accept(IProcessHost host, Signal signal)
    {
      if (signal is FinalizeSignal)
      {
        host.Finalize(this);
        return OkSignal.Make(this);
      }
      if (signal is TerminateSignal)
      {
        var finish = signal as TerminateSignal;
        UpdateStatus(host, ProcessStatus.Terminated, "Terminated!!!!", App.TimeSource.UTCNow, finish.SysAbout);
        host.Update(this, true);
        return OkSignal.Make(this);
      }
      // a pochemy net try/cath i ERROR signal???? ili eto v drugom meste
      var result = DoAccept(host, signal);        //Do Accept razve ne doljen byt POSLE systemnix signalov??

      if (result != null) return result;

      // a gde ostalnie signali? Terminate etc...?

      if (signal is FinishSignal)
      {
        var finish = signal as FinishSignal;
        UpdateStatus(host, ProcessStatus.Finished, finish.Description, App.TimeSource.UTCNow, finish.SysAbout);
        host.Update(this, true);
        return OkSignal.Make(this);
      }

      if (signal is CancelSignal)
      {
        var finish = signal as CancelSignal;
        UpdateStatus(host, ProcessStatus.Canceled, "Canceled!!!", App.TimeSource.UTCNow, finish.SysAbout);
        host.Update(this, true);
        return OkSignal.Make(this);
      }

      return UnknownSignal.Make(this, signal);
    }

    protected abstract ResultSignal DoAccept(IProcessHost host, Signal signal);

    protected void UpdateStatus(IProcessHost host, ProcessStatus status, string description, DateTime timestamp, string about)
    {
      m_SysDescriptor = new ProcessDescriptor(SysDescriptor, status, description, timestamp, about);
    }

    public void ValidateAndPrepareForSpawn(string targetName)
    {
      DoPrepareForEnqueuePreValidate(targetName);

        var ve = this.Validate(targetName);
        if (ve != null)
          throw new WorkersException(ServerStringConsts.ARGUMENT_ERROR + "Process.ValidateAndPrepareForEnqueue(todo).validate: " + ve.ToMessageWithType(), ve);

      DoPrepareForEnqueuePostValidate(targetName);
    }

    public override ValidState Validate(ValidState state, string scope = null)
    {
      state = base.Validate(state, scope);
      if (state.ShouldStop) return state;
      //todo
      return state;
    }

    protected virtual void DoPrepareForEnqueuePreValidate(string targetName) { }
    protected virtual void DoPrepareForEnqueuePostValidate(string targetName) { }
  }
}
