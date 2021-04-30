/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;
using System.Linq;

using Azos.Conf;
using Azos.Time;

namespace Azos.Apps
{
  /// <summary>
  /// Represents a lightweight daemon(background in-app process) which can be controlled by
  /// Start/SignalStop-like commands. This class serves a a base for various implementations
  /// (e.g. LogDaemon) including their composites. The base class state machine is thread-safe
  /// </summary>
  public abstract class Daemon : ApplicationComponent, IDaemon, ILocalizedTimeProvider
  {
    #region CONSTS

    public const string CONFIG_NAME_ATTR = "name";

    #endregion

    #region .ctor
    protected Daemon(IApplication application) : base(application)
    {
    }

    protected Daemon(IApplicationComponent director) : base(director)
    {
    }

    protected override void Destructor()
    {
      SignalStop();
      WaitForCompleteStop();

      base.Destructor();
    }

    #endregion

    #region Private Fields

    private object m_StatusLock = new object();
    private volatile DaemonStatus m_Status;

    private volatile bool m_PendingWaitingStop;

    [Config]
    private string m_Name;

    [Config(TimeLocation.CONFIG_TIMELOCATION_SECTION)]
    private TimeLocation m_TimeLocation = new TimeLocation();

    #endregion

    #region Properties

    /// <summary>
    /// Checks whether the class should not be auto-started by the application on boot.
    /// Default implementation checks whether it is decorated with ApplicationDontAutoStartDaemonAttribute
    /// </summary>
    public virtual bool ApplicationDontAutoStartDaemon
      => Attribute.IsDefined(GetType(), typeof(ApplicationDontAutoStartDaemonAttribute));

    /// <summary>
    /// Current daemon operational status: Inactive, Running etc...
    /// </summary>
    public DaemonStatus Status => m_Status;

    /// <summary>
    /// Provides short textual service description which is typically used by hosting apps
    /// to describe the services provided to callers.
    /// Usually this lists end-points that the server is listening on etc..
    /// </summary>
    public virtual string ServiceDescription => GetType().DisplayNameWithExpandedGenericArgs();

    /// <summary>
    /// Provides short textual current service status description, e.g. Daemons report their Status property
    /// </summary>
    public virtual string StatusDescription => Status.ToString();

    /// <summary>
    /// Returns true when daemon is non disposed, active or about to become active.
    /// You can poll this as a cancellation token in daemon implementation loops/threads/tasks
    /// </summary>
    public bool Running
    {
      get
      {
        var status = m_Status;
        return !Disposed && (status == DaemonStatus.Active || status == DaemonStatus.Starting);
      }
    }

    /// <summary>
    /// Provides textual name for the daemon
    /// </summary>
    public string Name
    {
      get { return m_Name.IsNullOrWhiteSpace() ? GetType().Name : m_Name; }
      protected set { m_Name = value; }
    }

    /// <summary>
    /// Returns time location of this LocalizedTimeProvider implementation
    /// </summary>
    public TimeLocation TimeLocation
    {
      get { return m_TimeLocation ?? TimeLocation.Parent;}
      set { m_TimeLocation = value; }
    }

    /// <summary>
    /// Returns current time localized per TimeLocation
    /// </summary>
    public DateTime LocalizedTime => UniversalTimeToLocalizedTime(App.TimeSource.UTCNow);

    #endregion

    #region Public

    /// <summary>
    /// Configures daemon from configuration node (and possibly it's sub-nodes)
    /// </summary>
    public void Configure(IConfigSectionNode fromNode)
    {
      EnsureObjectNotDisposed();
      lock (m_StatusLock)
      {
          CheckDaemonInactive();
          ConfigAttribute.Apply(this, fromNode);
          DoConfigure(fromNode);
      }
    }

    /// <summary>
    /// Blocking call that starts the daemon instance if it is not decorated by [DontAutoStartDaemon]
    /// </summary>
    internal bool StartByApplication()
    {
      if (ApplicationDontAutoStartDaemon) return false;
      Start();
      return true;
    }

    /// <summary>
    /// Blocking call that starts the daemon instance
    /// </summary>
    public void Start()
    {
      EnsureObjectNotDisposed();
      lock (m_StatusLock)
          if (m_Status == DaemonStatus.Inactive)
          {
              m_Status = DaemonStatus.Starting;
              try
              {
                Behavior.ApplyBehaviorAttributes(this);
                DoStart();
                m_Status = DaemonStatus.Active;
              }
              catch
              {
                m_Status = DaemonStatus.Inactive;
                throw;
              }
          }
    }

    /// <summary>
    /// Non-blocking call that initiates the stopping of the daemon
    /// </summary>
    public void SignalStop()
    {
      lock (m_StatusLock)
          if (m_Status == DaemonStatus.Active)
          {
              m_Status = DaemonStatus.Stopping;
              DoSignalStop();
          }
    }

    /// <summary>
    /// Non-blocking call that returns true when the daemon instance has completely stopped after SignalStop()
    /// </summary>
    public bool CheckForCompleteStop()
    {
      lock (m_StatusLock)
      {
          if (m_Status == DaemonStatus.Inactive) return true;

          if (m_Status == DaemonStatus.Stopping)
              return DoCheckForCompleteStop();
          else
              return false;
      }
    }

    /// <summary>
    /// Blocks execution of current thread until this daemon has completely stopped.
    /// This call must be performed by only 1 thread otherwise exception is thrown
    /// </summary>
    public void WaitForCompleteStop()
    {
      lock (m_StatusLock)
      {
          if (m_Status == DaemonStatus.Inactive) return;

          if (m_Status != DaemonStatus.Stopping) SignalStop();

          if (m_PendingWaitingStop) throw new AzosException(StringConsts.DAEMON_INVALID_STATE + "{0}.{1}".Args(Name,"WaitForCompleteStop() already blocked"));

          m_PendingWaitingStop = true;
          try
          {
            DoWaitForCompleteStop();
          }
          finally
          {
            m_PendingWaitingStop = false;
          }

          m_Status = DaemonStatus.Inactive;
      }
    }

    /// <summary>
    /// Accepts a visit of a manager entity - this call is useful for periodic updates of daemon status,
    /// i.e.  when daemon does not have a thread of its own it can be periodically managed by some other daemon through this method.
    /// The default implementation of DoAcceptManagerVisit(object, DateTime) does nothing
    /// </summary>
    public void AcceptManagerVisit(object manager, DateTime managerNow)
    {
      if (!Running) return;
      //call rooted at non-virt method for future extensibility with state checks here
      EnsureObjectNotDisposed();
      DoAcceptManagerVisit(manager, managerNow);
    }

    /// <summary>
    /// Converts universal time to local time as of TimeLocation property
    /// </summary>
    public DateTime UniversalTimeToLocalizedTime(DateTime utc)
    {
      if (utc.Kind!=DateTimeKind.Utc)
        throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".UniversalTimeToLocalizedTime(utc.Kind!=UTC)");

      var loc = TimeLocation;
      if (!loc.UseParentSetting)
      {
          return DateTime.SpecifyKind(utc + loc.UTCOffset, DateTimeKind.Local);
      }
      else
      {
        if (ComponentDirector is ILocalizedTimeProvider)
          return ((ILocalizedTimeProvider)ComponentDirector).UniversalTimeToLocalizedTime(utc);

        return App.UniversalTimeToLocalizedTime(utc);
      }
    }

    /// <summary>
    /// Converts localized time to UTC time as of TimeLocation property
    /// </summary>
    public DateTime LocalizedTimeToUniversalTime(DateTime local)
    {
      if (local.Kind!=DateTimeKind.Local)
        throw new TimeException(StringConsts.ARGUMENT_ERROR+GetType().Name+".LocalizedTimeToUniversalTime(utc.Kind!=Local)");

      var loc = TimeLocation;
      if (!loc.UseParentSetting)
      {
          return DateTime.SpecifyKind(local - loc.UTCOffset, DateTimeKind.Utc);
      }
      else
      {
          if (ComponentDirector is ILocalizedTimeProvider)
          return ((ILocalizedTimeProvider)ComponentDirector).LocalizedTimeToUniversalTime(local);

        return App.LocalizedTimeToUniversalTime(local);
      }
    }

    public override string ToString()
      => "Daemon {0}('{1}' @{2})".Args(GetType().DisplayNameWithExpandedGenericArgs(), Name, ComponentSID);

    #endregion

    #region Protected

    /// <summary>
    /// Allows to abort unsuccessful DoStart() overridden implementation.
    /// This method must be called from within DoStart()
    /// </summary>
    protected void AbortStart()
    {
        var trace = new StackTrace(1, false);

        if (!trace.GetFrames().Any(f => f.GetMethod().Name.Equals("DoStart", StringComparison.Ordinal)))
            Debugging.Fail(
                text: "Daemon.AbortStart() must be called from within DoStart()",
                action: DebugAction.ThrowAndLog);

        m_Status = DaemonStatus.AbortingStart;
    }

    /// <summary>
    /// Provides implementation that starts the daemon
    /// </summary>
    protected virtual void DoStart()
    {
    }

    /// <summary>
    /// Provides implementation that signals daemon to stop. This is expected not to block
    /// </summary>
    protected virtual void DoSignalStop()
    {
    }

    /// <summary>
    /// Provides implementation for checking whether the daemon has completely stopped
    /// </summary>
    protected virtual bool DoCheckForCompleteStop()
      => m_Status == DaemonStatus.Inactive;

    /// <summary>
    /// Provides implementation for a blocking call that returns only after a complete daemon stop
    /// </summary>
    protected virtual void DoWaitForCompleteStop()
    {
    }

    /// <summary>
    /// Provides implementation that configures daemon from configuration node (and possibly it's sub-nodes)
    /// </summary>
    protected virtual void DoConfigure(IConfigSectionNode node)
    {

    }

    /// <summary>
    /// Checks for daemon activity and throws exception if daemon is not in DaemonStatus.Active state
    /// </summary>
    protected void CheckDaemonActiveOrStarting()
    {
        if (m_Status!=DaemonStatus.Active && m_Status!=DaemonStatus.Starting)
        throw new AzosException(StringConsts.DAEMON_INVALID_STATE + Name);
    }

    /// <summary>
    /// Checks that daemon is not active and returns the passed variable throwing otherwise. Used for one line property setters
    /// </summary>
    protected T SetOnInactiveDaemon<T>(T value)
    {
        CheckDaemonInactive();
        return value;
    }

    /// <summary>
    /// Checks for daemon inactivity and throws exception if daemon is running (started, starting or stopping)
    /// </summary>
    protected void CheckDaemonInactive()
    {
        if (Status!= DaemonStatus.Inactive)
        throw new AzosException(StringConsts.DAEMON_INVALID_STATE + Name);
    }

    /// <summary>
    /// Checks for daemon activity and throws exception if daemon is not in DaemonStatus.Active state
    /// </summary>
    protected void CheckDaemonActive()
    {
        if (m_Status!=DaemonStatus.Active)
        throw new AzosException(StringConsts.DAEMON_INVALID_STATE + Name);
    }

    /// <summary>
    /// Accepts a visit from external manager. Base implementation does nothing.
    ///  Override in daemons that need external management calls
    ///   to update their state periodically, i.e. when they don't have a thread on their own
    /// </summary>
    protected virtual void DoAcceptManagerVisit(object manager, DateTime managerNow)
    {

    }

    #endregion
  }

  /// <summary>
  /// Represents daemon with typed ComponentDirector property
  /// </summary>
  public abstract class Daemon<TDirector> : Daemon where TDirector : IApplicationComponent
  {
    protected Daemon(IApplication application) : base(application) { }
    protected Daemon(TDirector director) : base(director) { }

    public new TDirector ComponentDirector => (TDirector)base.ComponentDirector;
  }
}
