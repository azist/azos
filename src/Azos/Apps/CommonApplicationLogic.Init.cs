/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Apps.Injection;
using Azos.Apps.Volatile;
using Azos.Log;
using Azos.Instrumentation;
using Azos.Conf;
using Azos.Data.Access;
using Azos.Glue;
using Azos.Security;
using Azos.Time;

namespace Azos.Apps
{
  partial class CommonApplicationLogic
  {
    protected virtual void InitApplication()
    {
      if (ForceInvariantCulture)//used in all server applications
        Platform.Abstraction.PlatformAbstractionLayer.SetProcessInvariantCulture();

      var exceptions = new List<Exception>();

      var starters = GetStarters().ToList();

      string name = CoreConsts.UNKNOWN;
      bool breakOnError = true;
      foreach (var starter in starters)
      {
        try
        {
          breakOnError = starter.ApplicationStartBreakOnException;
          name = starter.Name ?? starter.GetType().FullName;
          starter.ApplicationStartBeforeInit(this);
        }
        catch (Exception error)
        {
          error = new AzosException(StringConsts.APP_STARTER_BEFORE_ERROR.Args(name, error.ToMessageWithType()), error);
          if (breakOnError) throw error;
          exceptions.Add(error);
          //log not available at this point
        }
      }

      ExecutionContext.__BindApplication(this);
      DoInitApplication(); //<----------------------------------------------
      DoModuleAfterInitApplication();

      name = CoreConsts.UNKNOWN;
      breakOnError = true;
      foreach (var starter in starters)
      {
        try
        {
          breakOnError = starter.ApplicationStartBreakOnException;
          name = starter.Name ?? starter.GetType().FullName;
          starter.ApplicationStartAfterInit(this);
        }
        catch (Exception error)
        {
          error = new AzosException(StringConsts.APP_STARTER_AFTER_ERROR.Args(name, error.ToMessageWithType()), error);
          WriteLog(MessageType.CatastrophicError, "InitApplication().After", error.ToMessageWithType(), error);
          if (breakOnError) throw error;
        }
      }

      if (exceptions.Count > 0)
        foreach (var exception in exceptions)
          WriteLog(MessageType.CatastrophicError, "InitApplication().Before", exception.ToMessageWithType());
    }

    const string INIT_FROM = "app.ini";

    protected virtual void DoInitApplication()
    {
      ConfigAttribute.Apply(this, m_ConfigRoot);

      m_Name = m_ConfigRoot.AttrByName(CONFIG_NAME_ATTR).ValueAsString(GetType().FullName);

      var appid = m_ConfigRoot.AttrByName(CONFIG_ID_ATTR).Value;
      if (appid.IsNotNullOrWhiteSpace())
      {
        m_AppId = Atom.Encode(appid);
      }

      m_CloudOrigin = m_ConfigRoot.AttrByName(CONFIG_CLOUD_ORIGIN_ATTR).ValueAsAtom(Atom.ZERO);
      m_NodeDiscriminator = m_ConfigRoot.AttrByName(CONFIG_NODE_DISCRIMINATOR_ATTR).ValueAsUShort(0);

      Debugging.DefaultDebugAction = Debugging.ReadDefaultDebugActionFromConfig();
      Debugging.TraceDisabled = Debugging.ReadTraceDisableFromConfig();

      //20200616 DKh
      Serialization.Bix.Bixer.RegisterFromConfiguration(m_ConfigRoot[Serialization.Bix.Bixer.CONFIG_AZOS_SERIALIZATION_BIX_SECTION]);


      //the order of root component boot is important:

      InitLog();             //1.  must be the first one so others can log
      InitModule();          //2.  other services may use module references
      InitTimeSource();      //3.  start accurate time asap
      InitSecurityManager(); //4.  security context
      InitEventTimer();      //5.  event scheduler/bg jobs
      InitInstrumentation(); //6.  instrumentation
      InitDataStore();       //7.  data store
      InitObjectStore();     //8.  object store
      InitGlue();            //9.  glue
      InitDependencyInjector();//10.  custom dep injector last

      //After all inits apply the behavior top the root
      try
      {
        Behavior.ApplyConfiguredBehaviors(this, m_ConfigRoot);
        Behavior.ApplyBehaviorAttributes(this);
      }
      catch (Exception error)
      {
        var msg = StringConsts.APP_APPLY_BEHAVIORS_ERROR + error.ToMessageWithType();
        WriteLog(MessageType.CatastrophicError, INIT_FROM, msg, error);
        throw new AzosException(msg, error);
      }
    }

    protected virtual void DoModuleAfterInitApplication()
    {
      const string FROM = INIT_FROM + ".mod";

      if (m_Module != null)
      {
        WriteLog(MessageType.Trace, FROM, "Call module root DI and .ApplicationAfterInit()");
        try
        {
          this.DependencyInjector.InjectInto(m_Module);
        }
        catch (Exception error)
        {
          var msg = "Error performing DI into module root:" + error.ToMessageWithType();
          WriteLog(MessageType.CatastrophicError, FROM, msg, error);
          throw new AzosException(msg, error);
        }
        try
        {
          m_Module.ApplicationAfterInit();
        }
        catch (Exception error)
        {
          var msg = "Error in call module root .ApplicationAfterInit()" + error.ToMessageWithType();
          WriteLog(MessageType.CatastrophicError, FROM, msg, error);
          throw new AzosException(msg, error);
        }
      }

      WriteLog(MessageType.Trace, FROM, "Common application initialized in '{0}' time location".Args(this.TimeLocation));

      var related = WriteLog(MessageType.Trace, FROM, "Component dump:");

      foreach (var cmp in ApplicationComponent.AllComponents(this))
        WriteLog(MessageType.Info,
                 FROM,
                 "  -> Component: {0}  '{1}'  '{2}' ".Args(cmp.ComponentSID, cmp.GetType().FullName, cmp.ComponentCommonName),
                 related: related);
    }

    //Must be called first to boot log before all other components
    protected virtual void InitLog()
    {
      var node = m_ConfigRoot[CONFIG_LOG_SECTION];
      if (!node.Exists) return;

      try
      {
        m_Log = FactoryUtils.MakeAndConfigureComponent<ILogImplementation>(this, node, typeof(LogDaemon));

        WriteLog(MessageType.Trace, INIT_FROM, "Log made");

        if (m_Log is Daemon daemon)
          if (daemon.StartByApplication())
            WriteLog(MessageType.Trace, INIT_FROM, "Log daemon started, msg times are localized to machine-local time until time source starts");
      }
      catch (Exception error)
      {
        //this is log, so we cant log anywhere but throw
        throw new AzosException(StringConsts.APP_LOG_INIT_ERROR + error.ToMessageWithType(), error);
      }
    }

    protected virtual void InitModule()
    {
      var node = m_ConfigRoot[CONFIG_MODULES_SECTION];
      if (!node.Exists) return;

      try
      {
        m_Module = FactoryUtils.MakeAndConfigureComponent<IModuleImplementation>(this, node, typeof(HubModule));

        WriteLog(MessageType.Trace, INIT_FROM, "Module root made");

        if (m_Module is Daemon daemon)
          if (daemon.StartByApplication())
            WriteLog(MessageType.Trace, INIT_FROM, "Module root daemon started");
      }
      catch (Exception error)
      {
        var msg = StringConsts.APP_MODULE_INIT_ERROR + error.ToMessageWithType();
        WriteLog(MessageType.CatastrophicError, INIT_FROM, msg, error);
        throw new AzosException(msg, error);
      }
    }

    protected virtual void InitTimeSource()
    {
      var node = m_ConfigRoot[CONFIG_TIMESOURCE_SECTION];
      if (!node.Exists)
      {
        WriteLog(MessageType.Trace, INIT_FROM, "Using default time source");

        m_StartTime = LocalizedTime;
        WriteLog(MessageType.Info, INIT_FROM, "App start time is {0}".Args(m_StartTime));
        return;
      }

      try
      {
        m_TimeSource = FactoryUtils.MakeAndConfigureComponent<ITimeSourceImplementation>(this, node);

        WriteLog(MessageType.Trace, INIT_FROM, "TimeSource made");

        if (m_TimeSource is Daemon daemon)
          if (daemon.StartByApplication())
            WriteLog(MessageType.Trace, INIT_FROM, "TimeSource daemon started");

        WriteLog(MessageType.Info, INIT_FROM, "Log msg time is supplied by time source now");
        m_StartTime = LocalizedTime;
        WriteLog(MessageType.Info, INIT_FROM, "App start time is {0}".Args(m_StartTime));
      }
      catch (Exception error)
      {
        var msg = StringConsts.APP_TIMESOURCE_INIT_ERROR + error.ToMessageWithType();
        WriteLog(MessageType.CatastrophicError, INIT_FROM, msg, error);
        throw new AzosException(msg, error);
      }
    }

    protected virtual void InitEventTimer()
    {
      var node = m_ConfigRoot[CONFIG_EVENT_TIMER_SECTION];
      //Event timer must be allocated even if it is absent in config
      try
      {
        m_EventTimer = FactoryUtils.MakeAndConfigureComponent<IEventTimerImplementation>(this, node, typeof(EventTimer));

        WriteLog(MessageType.Trace, INIT_FROM, "EventTimer made");

        if (m_EventTimer is Daemon daemon)
          if (daemon.StartByApplication())
            WriteLog(MessageType.Trace, INIT_FROM, "EventTimer daemon started");
      }
      catch (Exception error)
      {
        var msg = StringConsts.APP_EVENT_TIMER_INIT_ERROR + error.ToMessageWithType();
        WriteLog(MessageType.CatastrophicError, INIT_FROM, msg, error);
        throw new AzosException(msg, error);
      }
    }

    protected virtual void InitSecurityManager()
    {
      var node = m_ConfigRoot[CONFIG_SECURITY_SECTION];
      if (!node.Exists) return;

      try
      {
        m_SecurityManager = FactoryUtils.MakeAndConfigureComponent<ISecurityManagerImplementation>(this, node, typeof(ConfigSecurityManager));

        WriteLog(MessageType.Trace, INIT_FROM, "Secman made");

        if (m_SecurityManager is Daemon daemon)
          if (daemon.StartByApplication())
            WriteLog(MessageType.Trace, INIT_FROM, "Secman daemon started");
      }
      catch (Exception error)
      {
        var msg = StringConsts.APP_SECURITY_MANAGER_INIT_ERROR + error.ToMessageWithType();
        WriteLog(MessageType.CatastrophicError, INIT_FROM, msg, error);
        throw new AzosException(msg, error);
      }
    }

    protected virtual void InitInstrumentation()
    {
      var node = m_ConfigRoot[CONFIG_INSTRUMENTATION_SECTION];
      if (!node.Exists) return;

      try
      {
        m_Instrumentation = FactoryUtils.MakeAndConfigureComponent<IInstrumentationImplementation>(this, node, typeof(InstrumentationDaemon));

        WriteLog(MessageType.Trace, INIT_FROM, "Instr made");

        if (m_Instrumentation is Daemon daemon)
          if (daemon.StartByApplication())
            WriteLog(MessageType.Trace, INIT_FROM, "Instr daemon started");
      }
      catch (Exception error)
      {
        var msg = StringConsts.APP_INSTRUMENTATION_INIT_ERROR + error.ToMessageWithType();
        WriteLog(MessageType.CatastrophicError, INIT_FROM, msg, error);
        throw new AzosException(msg, error);
      }
    }

    protected virtual void InitDataStore()
    {
      var node = m_ConfigRoot[CONFIG_DATA_STORE_SECTION];
      if (!node.Exists) return;

      try
      {
        m_DataStore = FactoryUtils.MakeAndConfigureComponent<IDataStoreImplementation>(this, node);

        WriteLog(MessageType.Trace, INIT_FROM, "Datastore made");

        if (m_DataStore is Daemon daemon)
          if (daemon.StartByApplication())
            WriteLog(MessageType.Trace, INIT_FROM, "Datastore daemon started");
      }
      catch (Exception error)
      {
        var msg = StringConsts.APP_DATA_STORE_INIT_ERROR + error.ToMessageWithType();
        WriteLog(MessageType.CatastrophicError, INIT_FROM, msg, error);
        throw new AzosException(msg, error);
      }
    }

    protected virtual void InitObjectStore()
    {
      var node = m_ConfigRoot[CONFIG_OBJECT_STORE_SECTION];
      if (!node.Exists) return;

      try
      {
        m_ObjectStore = FactoryUtils.MakeAndConfigureComponent<IObjectStoreImplementation>(this, node, typeof(ObjectStoreDaemon));

        WriteLog(MessageType.Trace, INIT_FROM, "Objectstore made");

        if (m_ObjectStore is Daemon daemon)
          if (daemon.StartByApplication())
            WriteLog(MessageType.Trace, INIT_FROM, "Objectstore daemon started");
      }
      catch (Exception error)
      {
        var msg = StringConsts.APP_OBJECT_STORE_INIT_ERROR + error.ToMessageWithType();
        WriteLog(MessageType.CatastrophicError, INIT_FROM, msg, error);
        throw new AzosException(msg, error);
      }
    }

    protected virtual void InitGlue()
    {
      var node = m_ConfigRoot[CONFIG_GLUE_SECTION];
      if (!node.Exists) return;

      try
      {
        m_Glue = FactoryUtils.MakeAndConfigureComponent<IGlueImplementation>(this, node, typeof(Glue.Implementation.GlueDaemon));

        WriteLog(MessageType.Trace, INIT_FROM, "Glue made");

        if (m_Glue is Daemon daemon)
          if (daemon.StartByApplication())
            WriteLog(MessageType.Trace, INIT_FROM, "Glue daemon started");
      }
      catch (Exception error)
      {
        var msg = StringConsts.APP_GLUE_INIT_ERROR + error.ToMessageWithType();
        WriteLog(MessageType.CatastrophicError, INIT_FROM, msg, error);
        throw new AzosException(msg, error);
      }
    }

    protected virtual void InitDependencyInjector()
    {
      var node = m_ConfigRoot[CONFIG_DEPENDENCY_INJECTOR_SECTION];
      if (!node.Exists) return;

      try
      {
        m_DependencyInjector = FactoryUtils.MakeAndConfigureComponent<IApplicationDependencyInjectorImplementation>(this, node, typeof(ApplicationDependencyInjector));

        WriteLog(MessageType.Trace, INIT_FROM, "DI made");

        if (m_DependencyInjector is Daemon daemon)
          if (daemon.StartByApplication())
            WriteLog(MessageType.Trace, INIT_FROM, "DI daemon started");
      }
      catch (Exception error)
      {
        var msg = StringConsts.APP_DI_INIT_ERROR + error.ToMessageWithType();
        WriteLog(MessageType.CatastrophicError, INIT_FROM, msg, error);
        throw new AzosException(msg, error);
      }
    }

  }
}
