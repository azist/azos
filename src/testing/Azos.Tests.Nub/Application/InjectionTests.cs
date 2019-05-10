/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Scripting;
using Azos.Log;
using Azos.Data.Access;
using Azos.Glue;
using Azos.Time;
using Azos.Instrumentation;
using Azos.Serialization.Slim;
using System.IO;

namespace Azos.Tests.Nub.Application
{
  [Runnable]
  public class InjectionTests
  {
    static readonly ConfigSectionNode BASE_CONF = @"
  app{
    modules
    {
      module{type='Azos.Tests.Nub.Application.InjectionTests+MyModule, Azos.Tests.Nub' }
      module{type='Azos.Tests.Nub.Application.InjectionTests+MyModule, Azos.Tests.Nub' name='Module2'}
      module{type='Azos.Tests.Nub.Application.InjectionTests+MyModule, Azos.Tests.Nub' name='Module3'}
    }
  }
  ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);


    [Run]
    public void Test_InjectionTarget_Root()
    {
      using( var app = new AzosApplication(null, BASE_CONF))
      {
        var target = new InjectionTarget_Root();
        app.DependencyInjector.InjectInto(target);
        target.AssertInjectionCorrectness(app);
      }
    }

    [Run]
    public void Test_InjectionTarget_Modules()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var target = new InjectionTarget_Modules();
        app.DependencyInjector.InjectInto(target);
        target.AssertInjectionCorrectness(app);
      }
    }

    [Run]
    public void Test_InjectionTarget_Singleton()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var singleton = app.Singletons.GetOrCreate( () => new Dictionary<string, string>() );

        var target = new InjectionTarget_Singleton();
        app.DependencyInjector.InjectInto(target);
        target.AssertInjectionCorrectness(app, singleton.instance);
      }
    }

    [Run]
    public void Test_InjectionTarget_Serialization()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        const string DATA = "lalala!";

        var target = new InjectionTarget_Root();
        target.Data = DATA;

        app.DependencyInjector.InjectInto(target);
        target.AssertInjectionCorrectness(app);

        using(var ms = new MemoryStream())
        {
          var ser= new SlimSerializer();
          ser.Serialize(ms, target);
          ms.Position = 0;

          var target2 = ser.Deserialize(ms) as InjectionTarget_Root;
          Aver.IsNotNull(target2);//we deserialized the instance

          Aver.AreEqual(DATA, target2.Data);//the Data member got deserialized ok
          Aver.AreNotSameRef(target.Data, target2.Data);
          Aver.AreNotSameRef(DATA, target2.Data);

          target2.AssertAllInjectionsNull();//but all injections are transitive, hence are null
          app.DependencyInjector.InjectInto(target2);
          target2.AssertInjectionCorrectness(app);//and are re-hydrated again after InjectInto() call
        }
      }
    }


    [Run]
    public void Test_InjectionTarget_AppInjection_True()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var target = new InjectionTarget_AppInjection_True();
        app.DependencyInjector.InjectInto(target);

        target.AssertAllInjectionsNull();
        Aver.AreSameRef(app, target.AppInjectedByHand);
        Aver.AreEqual(app.DependencyInjector.ComponentSID, target.InjectorSID);
      }
    }

    [Run]
    public void Test_InjectionTarget_AppInjection_False()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var target = new InjectionTarget_AppInjection_False();
        app.DependencyInjector.InjectInto(target);

        target.AssertInjectionCorrectness(app);
        Aver.AreEqual(app.DependencyInjector.ComponentSID, target.InjectorSID);
      }
    }


    interface IMyModule : IModule { }

    public class MyModule : ModuleBase, IMyModule
    {
      public MyModule(IApplication app) : base(app) { }
      public MyModule(IModule parent) : base(parent) { }
      public override bool IsHardcodedModule => false;
      public override string ComponentLogTopic => "testing";
    }
    public class InjectionTarget_Root
    {
#pragma warning disable 649
      [Inject] IApplication m_App;//<--- private field
      [Inject] public IApplication m_App2;
      [Inject] protected IApplication m_App3;

      [Inject] ILog m_LogAsLog;
      [Inject(Type = typeof(ILog))] object m_LogAsObject;
      [Inject] IDataStore m_DataStore;
      [Inject] IGlue m_Glue;
      [Inject] IInstrumentation m_Instrumentation;
      [Inject] ITimeSource m_TimeSource;
#pragma warning restore 649

      public string Data;//<-- this gotta be serializable

      public virtual void AssertInjectionCorrectness(IApplication app)
      {
        Aver.AreSameRef(app, m_App);
        Aver.AreSameRef(app, m_App2);
        Aver.AreSameRef(app, m_App3);

        Aver.AreSameRef(app.Log, m_LogAsLog);
        Aver.AreSameRef(app.Log, m_LogAsObject);
        Aver.AreSameRef(app.DataStore, m_DataStore);
        Aver.AreSameRef(app.Glue, m_Glue);
        Aver.AreSameRef(app.Instrumentation, m_Instrumentation);
        Aver.AreSameRef(app.TimeSource, m_TimeSource);
      }

      public virtual void AssertAllInjectionsNull()
      {
        Aver.IsNull( m_App);
        Aver.IsNull( m_App2);
        Aver.IsNull( m_App3);
        Aver.IsNull(m_LogAsLog);
        Aver.IsNull(m_LogAsObject);
        Aver.IsNull(m_DataStore);
        Aver.IsNull(m_Glue);
        Aver.IsNull(m_Instrumentation);
        Aver.IsNull(m_TimeSource);
      }
    }

    public class InjectionTarget_Modules : InjectionTarget_Root
    {
#pragma warning disable 649
      [Inject] IMyModule m_MyModule1;
      [Inject(Name="Module2")] IMyModule m_MyModule2;
      [InjectModule(Name = "Module3")] IMyModule m_MyModule3;
#pragma warning restore 649
      public override void AssertInjectionCorrectness(IApplication app)
      {
        base.AssertInjectionCorrectness(app);
        Aver.AreSameRef(app.ModuleRoot.Get<IMyModule>(), m_MyModule1);
        Aver.AreSameRef(app.ModuleRoot.Get<IMyModule>("Module2"), m_MyModule2);
        Aver.AreSameRef(app.ModuleRoot.Get<IMyModule>("Module3"), m_MyModule3);
      }
    }

    public class InjectionTarget_Singleton : InjectionTarget_Modules
    {
#pragma warning disable 649
      [InjectSingleton] Dictionary<string, string> m_MySingleton1;
      [InjectSingleton(Type =typeof(Dictionary<string, string>))] IDictionary<string, string> m_MySingleton2;
#pragma warning restore 649
      public void AssertInjectionCorrectness(IApplication app, Dictionary<string, string> dict)
      {
        base.AssertInjectionCorrectness(app);
        Aver.AreSameRef(dict, m_MySingleton1);
        Aver.AreSameRef(dict, m_MySingleton2);
      }
    }

    public class InjectionTarget_AppInjection_True : InjectionTarget_Root, IApplicationInjection
    {
      public ulong InjectorSID;
      public IApplication AppInjectedByHand;

      public bool InjectApplication(IApplicationDependencyInjector injector)
      {
        AppInjectedByHand = injector.App;
        InjectorSID = injector.ComponentSID;
        return true;//completed do not inject anything else
      }
    }

    public class InjectionTarget_AppInjection_False : InjectionTarget_Root, IApplicationInjection
    {
      public ulong InjectorSID;

      public bool InjectApplication(IApplicationDependencyInjector injector)
      {
        InjectorSID = injector.ComponentSID;
        return false;//not completed keep injecting
      }
    }

  }
}