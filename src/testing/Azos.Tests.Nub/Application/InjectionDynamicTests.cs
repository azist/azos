/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.IO;
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

namespace Azos.Tests.Nub.Application
{
  [Runnable]
  public class InjectionDynamicTests
  {
    static readonly ConfigSectionNode STANDARD_CONF = @"
  app{
    modules
    {
      module{type='Azos.Tests.Nub.Application.InjectionDynamicTests+MyModule, Azos.Tests.Nub' }
    }
  }
  ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

    static readonly ConfigSectionNode EMPTY_CONF = @"
  app{
    modules
    {
      // nothing loaded
    }
  }
  ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);


    [Run]
    public void Test_InjectByConfig()
    {
      using( var app = new AzosApplication(null, STANDARD_CONF))
      {
        var target = new InjectionTarget();
        app.DependencyInjector.InjectInto(target);
        target.AssertInjectionCorrectness(app, app.ModuleRoot.Get<IMyModule>());
      }
    }

    [Run]
    public void Test_InjectDynamic()
    {
      using (var app = new AzosApplication(null, EMPTY_CONF))
      {
        using(var mod = new MyModule(app))
        {
          DynamicModuleFlowScope.Begin();
          var target = new InjectionTarget();
          Aver.IsTrue(DynamicModuleFlowScope.Register(mod));
          app.DependencyInjector.InjectInto(target);
          target.AssertInjectionCorrectness(app, mod);
          Aver.IsTrue(DynamicModuleFlowScope.Unregister(mod));
        }
      }
    }

    public interface IMyModule : IModule { }

    public class MyModule : ModuleBase, IMyModule
    {
      public MyModule(IApplication app) : base(app) { }
      public MyModule(IModule parent) : base(parent) { }
      public override bool IsHardcodedModule => false;
      public override string ComponentLogTopic => "testing";
    }


    public class InjectionTarget
    {
      [Inject] IApplication m_App;
      [Inject] IMyModule m_MyModule;

      public virtual void AssertInjectionCorrectness(IApplication app, IMyModule mod)
      {
        Aver.AreSameRef(app, m_App);
        Aver.AreSameRef(mod, m_MyModule);
      }
    }
  }
}