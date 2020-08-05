/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Data;

namespace Azos.Sky.Workers
{
  [Serializable]
  public abstract class Signal : AmorphousTypedDoc
  {
    [Inject] ISkyApplication m_App;

    public ISkyApplication App => m_App.NonNull(nameof(m_App));

    /// <summary>
    /// Factory method that creates new Signal based on provided PID
    /// </summary>
    public static TSignal MakeNew<TSignal>(IApplication app, PID pid) where TSignal : Signal, new() { return makeDefault(app.AsSky(), new TSignal(), pid); }

    /// <summary>
    /// Factory method that creates new Signal based on provided Type, PID and Configuration
    /// </summary>
    public static Signal MakeNew(IApplication app, Type type, PID pid, IConfigSectionNode args) { return makeDefault(app.AsSky(), FactoryUtils.MakeAndConfigure<Signal>(args, type), pid); }

    private static TSignal makeDefault<TSignal>(ISkyApplication app, TSignal signal, PID pid) where TSignal : Signal
    {
      app.DependencyInjector.InjectInto(signal);
      signal.m_SysID = app.GdidProvider.GenerateOneGdid(SysConsts.GDID_NS_WORKER, SysConsts.GDID_NAME_WORKER_SIGNAL);
      signal.m_SysPID = pid;
      signal.m_SysTimestampUTC = app.TimeSource.UTCNow;
      signal.m_SysAbout = "{0}@{1}@{2}".Args(Ambient.CurrentCallUser.Name, app.Name, app.HostName);
      return signal;
    }

    protected Signal() { }

    private GDID m_SysID;
    private PID m_SysPID;
    private DateTime m_SysTimestampUTC;
    private string m_SysAbout;

    public void ____Deserialize(GDID id, PID pid, DateTime ts, string about)
    { m_SysID = id; m_SysPID = pid; m_SysTimestampUTC = ts; m_SysAbout = about; }

    /// <summary>
    /// Globally-unique ID of the Signal
    /// </summary>
    public GDID SysID { get { return m_SysID; } }

    /// <summary>
    /// Globally-unique ID of the Process
    /// </summary>
    public PID SysPID { get { return m_SysPID; } }

    /// <summary>
    /// When was created
    /// </summary>
    public DateTime SysCreateTimeStampUTC { get { return m_SysTimestampUTC; } }

    /// <summary>
    /// Who is creator
    /// </summary>
    public string SysAbout { get { return m_SysAbout; } }

    /// <summary>
    /// Type Guid
    /// </summary>
    public Guid SysTypeGuid { get { return GuidTypeAttribute.GetGuidTypeAttribute<Signal, SignalAttribute>(GetType()).TypeGuid; } }

    public override string ToString() { return "{0}('{1}')".Args(GetType().Name, SysPID); }

    public override int GetHashCode() { return m_SysID.GetHashCode(); }

    public override bool Equals(Doc other)
    {
      var otherSignal = other as Signal;
      if (otherSignal == null) return false;
      return this.m_SysID == otherSignal.m_SysID;
    }

    public void ValidateAndPrepareForDispatch(string targetName)
    {
      DoPrepareForEnqueuePreValidate(targetName);

      var ve = this.Validate(targetName);
      if (ve != null)
        throw new WorkersException(StringConsts.ARGUMENT_ERROR + "Signal.ValidateAndPrepareForEnqueue(todo).validate: " + ve.ToMessageWithType(), ve);

      DoPrepareForEnqueuePostValidate(targetName);
    }

    public override ValidState Validate(ValidState state, string scope = null)
    {
      state = base.Validate(state, scope);
      if (state.ShouldStop) return state;

      if (SysID.IsZero)
        state = new ValidState(state, new FieldValidationException(this, "SysID", "SysID.IsZero, use MakeNew<>() to make new instances"));

      return state;
    }

    protected virtual void DoPrepareForEnqueuePreValidate(string targetName) { }
    protected virtual void DoPrepareForEnqueuePostValidate(string targetName) { }
  }

  [Serializable]
  public abstract class ResultSignal : Signal
  {
    /// <summary>
    /// Factory method that creates new Result Signals assigning them new GDID
    /// </summary>
    public static TSignal MakeNew<TSignal>(Process process) where TSignal : ResultSignal, new()
    {
      var result = Signal.MakeNew<TSignal>(process.App, process.SysPID);
      result.m_SysDescriptor = process.SysDescriptor;
      return result;
    }

    private ProcessDescriptor m_SysDescriptor;

    public ProcessDescriptor SysDescriptor { get { return m_SysDescriptor; } }

    public void ____Deserialize(ProcessDescriptor descriptor)
    { m_SysDescriptor = descriptor; }
  }
}
