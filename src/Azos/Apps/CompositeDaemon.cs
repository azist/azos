/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Azos.Conf;
using Azos.Collections;
using Azos.Log;

namespace Azos.Apps
{
  /// <summary>
  /// Represents a daemon that contains other child daemons.
  /// Start/Stop commands translate into child sub-commands.
  /// This class is used to host other services in various job/background process hosts
  /// </summary>
  public class CompositeDaemon : Daemon
  {
    #region Inner

    /// <summary>
    /// Child daemon entry as managed by CompositeDaemon class
    /// </summary>
    public struct ChildDaemon : INamed, IOrdered
    {
      public ChildDaemon(Daemon svc, int order, bool abortStart)
      {
        Service = svc;
        Order = order;
        AbortStart = abortStart;
      }

      public Daemon Service { get; }
      public string Name  => Service.Name;
      public int Order { get; }
      public bool AbortStart { get; }

      public override string ToString() =>
        "{0}Service({1})[{2}]".Args(AbortStart ? "Abortable" : "", Name, Order);
    }

    #endregion

    #region CONSTS

    public const string CONFIG_DAEMON_COMPOSITE_SECTION = "daemon-composite";
    public const string CONFIG_DAEMON_SECTION = "daemon";
    public const string CONFIG_ABORT_START_ATTR = "abort-start";
    public const string CONFIG_IGNORE_THIS_DAEMON_ATTR = "ignore-this-daemon";

    #endregion

    #region .ctor

    public CompositeDaemon(IApplication application) : base(application) { }
    public CompositeDaemon(IApplicationComponent director) : base(director) { }

    protected override void Destructor()
    {
      base.Destructor();
      foreach (var csvc in m_Services.Where(s => s.Service.ComponentDirector == this))
        try
        {
          csvc.Service.Dispose();
        }
        catch (Exception error)
        {
          WriteLog(MessageType.Error, "Destructor('{0}')".Args(csvc), error.ToMessageWithType(), error);
        }
    }

    #endregion

    #region Fields

    private OrderedRegistry<ChildDaemon> m_Services = new OrderedRegistry<ChildDaemon>();

    #endregion

    #region Properties

    /// <summary>
    /// Returns service registry where services can be looked-up by name
    /// </summary>
    public IOrderedRegistry<ChildDaemon> ChildServices => m_Services;

    #endregion

    #region Public

    /// <summary>
    /// Returns true if child service was registered, false if it was already registered prior tp this call.
    /// The method may only be called on stopped (this) service
    /// </summary>
    public bool RegisterService(Daemon service, int order, bool abortStart)
    {
      CheckDaemonInactive();
      var csvc = new ChildDaemon(service, order, abortStart);
      return m_Services.Register(csvc);
    }

    /// <summary>
    /// Returns true if child service was unregistered, false if it did not exist.
    /// The method may only be called on stopped (this) service
    /// </summary>
    public bool UnregisterService(Daemon service)
    {
      CheckDaemonInactive();
      var csvc = new ChildDaemon(service, 0, false);
      return m_Services.Unregister(csvc);
    }

    #endregion

    #region Protected

    public override string ComponentLogTopic => CoreConsts.APPLICATION_TOPIC;

    protected override void DoConfigure(IConfigSectionNode node)
    {
      if (node == null)
        node = App.ConfigRoot[CONFIG_DAEMON_COMPOSITE_SECTION];

      foreach (var snode in node.Children
                              .Where(cn => cn.IsSameName(CONFIG_DAEMON_SECTION))
                              .OrderBy(cn => cn.AttrByName(Configuration.CONFIG_ORDER_ATTR).ValueAsInt(0)))//the order here is needed so that child services get CREATED in order,
                                                                                                            // not only launched in order
      {
        var ignored = snode.AttrByName(CONFIG_IGNORE_THIS_DAEMON_ATTR).ValueAsBool(false);
        if (ignored)
        {
          WriteLog(MessageType.Warning, nameof(DoConfigure), "Service {0} is ignored".Args(snode.AttrByName("name").Value));
          continue;
        }

        var svc = FactoryUtils.MakeAndConfigureDirectedComponent<Daemon>(this, snode);
        var abort = snode.AttrByName(CONFIG_ABORT_START_ATTR).ValueAsBool(true);
        RegisterService(svc, snode.AttrByName(Configuration.CONFIG_ORDER_ATTR).ValueAsInt(0), abort);
      }
    }

    protected override void DoStart()
    {
      var started = new List<Daemon>();

      foreach (var csvc in m_Services.OrderedValues)
        try
        {
          csvc.Service.Start();
          started.Add(csvc.Service);
        }
        catch (Exception error)
        {
          var guid = WriteLog(MessageType.CatastrophicError, "DoStart('{0}')".Args(csvc), error.ToMessageWithType(), error);

          if (csvc.AbortStart)
          {
            this.AbortStart();
            foreach (var service in started)
              try
              {
                service.WaitForCompleteStop();
              }
              catch (Exception ex)
              {
                WriteLog(MessageType.CriticalAlert, "DoStart('{0}').WaitForCompleteStop('{1}')".Args(csvc, service.GetType().Name), ex.ToMessageWithType(), ex, guid);
              }
            throw new CompositeDaemonException(StringConsts.DAEMON_COMPOSITE_CHILD_START_ABORT_ERROR.Args(csvc, error.ToMessageWithType()), error);
          }
        }
    }

    protected override void DoSignalStop()
    {
      foreach (var csvc in m_Services.OrderedValues.Reverse())
        try
        {
          csvc.Service.SignalStop();
        }
        catch (Exception error)
        {
          WriteLog(MessageType.Error, "DoSignalStop('{0}')".Args(csvc), error.ToMessageWithType(), error);
        }
    }

    protected override void DoWaitForCompleteStop()
    {
      foreach (var csvc in m_Services.OrderedValues.Reverse())
        try
        {
          csvc.Service.WaitForCompleteStop();
        }
        catch (Exception error)
        {
          WriteLog(MessageType.Error, "DoWaitForCompleteStop('{0}')".Args(csvc), error.ToMessageWithType(), error);
        }
    }

    protected override bool DoCheckForCompleteStop()
    {
      foreach (var csvc in m_Services.OrderedValues)
        try
        {
          if (!csvc.Service.CheckForCompleteStop()) return false;
        }
        catch (Exception error)
        {
          WriteLog(MessageType.Error, "DoCheckForCompleteStop('{0}')".Args(csvc), error.ToMessageWithType(), error);
        }
      return true;
    }

    protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
    {
      foreach (var csvc in m_Services.OrderedValues)
        try
        {
          csvc.Service.AcceptManagerVisit(manager, managerNow);
        }
        catch (Exception error)
        {
          WriteLog(MessageType.Error, "DoAcceptManagerVisit('{0}')".Args(csvc), error.ToMessageWithType(), error);
        }
    }

    #endregion
  }


  /// <summary>
  ///Thrown by CompositeDaemon
  /// </summary>
  [Serializable]
  public class CompositeDaemonException : AzosException
  {
    public CompositeDaemonException() { }
    public CompositeDaemonException(string message) : base(message) { }
    public CompositeDaemonException(string message, Exception inner) : base(message, inner) { }
    protected CompositeDaemonException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
