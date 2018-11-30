/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Scripting;
using Azos.Log;
using Azos.Data.Access;
using Azos.Glue;
using Azos.Time;
using Azos.Instrumentation;
using System.Collections.Generic;

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
      using( var app = new ServiceBaseApplication(null, BASE_CONF))
      {
        var target = new InjectionTarget_Root();
        app.DependencyInjector.InjectInto(target);
        target.AssertCorrectness(app);
      }
    }

    [Run]
    public void Test_InjectionTarget_Modules()
    {
      using (var app = new ServiceBaseApplication(null, BASE_CONF))
      {
        var target = new InjectionTarget_Modules();
        app.DependencyInjector.InjectInto(target);
        target.AssertCorrectness(app);
      }
    }

    [Run]
    public void Test_InjectionTarget_Singleton()
    {
      using (var app = new ServiceBaseApplication(null, BASE_CONF))
      {
        var singleton = app.Singletons.GetOrCreate( () => new Dictionary<string, string>() );

        var target = new InjectionTarget_Singleton();
        app.DependencyInjector.InjectInto(target);
        target.AssertCorrectness(app, singleton.instance);
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