/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


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
        target.AssertCorrectness(app);
      }
    }

    [Run]
    public void Test_InjectionTarget_Modules()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var target = new InjectionTarget_Modules();
        app.DependencyInjector.InjectInto(target);
        target.AssertCorrectness(app);
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
        target.AssertCorrectness(app, singleton.instance);
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
        target.AssertCorrectness(app);

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
          target2.AssertCorrectness(app);//and are re-hydrated again after InjectInto() call
        }
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
      [Inject] IApplication m_App;//<--- private field
      [Inject] public IApplication m_App2;
      [Inject] protected IApplication m_App3;

      [Inject] ILog m_LogAsLog;
      [Inject(Type = typeof(ILog))] object m_LogAsObject;
      [Inject] IDataStore m_DataStore;
      [Inject] IGlue m_Glue;
      [Inject] IInstrumentation m_Instrumentation;
      [Inject] ITimeSource m_TimeSource;

      public string Data;//<-- this gotta be serializable

      public virtual void AssertCorrectness(IApplication app)
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
      [Inject] IMyModule m_MyModule1;
      [Inject(Name="Module2")] IMyModule m_MyModule2;
      [InjectModule(Name = "Module3")] IMyModule m_MyModule3;

      public override void AssertCorrectness(IApplication app)
      {
        base.AssertCorrectness(app);
        Aver.AreSameRef(app.ModuleRoot.Get<IMyModule>(), m_MyModule1);
        Aver.AreSameRef(app.ModuleRoot.Get<IMyModule>("Module2"), m_MyModule2);
        Aver.AreSameRef(app.ModuleRoot.Get<IMyModule>("Module3"), m_MyModule3);
      }
    }

    public class InjectionTarget_Singleton : InjectionTarget_Modules
    {
      [InjectSingleton] Dictionary<string, string> m_MySingleton1;
      [InjectSingleton(Type =typeof(Dictionary<string, string>))] IDictionary<string, string> m_MySingleton2;

      public void AssertCorrectness(IApplication app, Dictionary<string, string> dict)
      {
        base.AssertCorrectness(app);
        Aver.AreSameRef(dict, m_MySingleton1);
        Aver.AreSameRef(dict, m_MySingleton2);
      }
    }

  }
}