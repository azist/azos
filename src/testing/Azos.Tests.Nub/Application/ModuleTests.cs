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
  public class ModuleTests
  {
    static readonly ConfigSectionNode BASE_CONF = @"
  app{
    modules
    {
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyModuleA, Azos.Tests.Nub' order=3}
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyModuleA, Azos.Tests.Nub' name='Module1' order=1}
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyModuleB, Azos.Tests.Nub' name='Module4' order=4}
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyModuleA, Azos.Tests.Nub' name='Module2' order=2}
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyServiceA, Azos.Tests.Nub' name = 's1'}
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyServiceB, Azos.Tests.Nub' name = 's2'}
    }
  }
  ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);



    [Run]
    public void Test_ModuleInjection()
    {
      using( var app = new ServiceBaseApplication(null, BASE_CONF))
      {
        Aver.AreEqual(6, app.ModuleRoot.ChildModules.Count);

        Aver.AreEqual("s1", app.ModuleRoot.ChildModules[0].Name);//their order is not specified hence 0
        Aver.AreEqual("s2", app.ModuleRoot.ChildModules[1].Name);

        Aver.AreEqual("Module1", app.ModuleRoot.ChildModules[2].Name);
        Aver.AreEqual("Module2", app.ModuleRoot.ChildModules[3].Name);
        Aver.AreEqual("Azos.Tests.Nub.Application.ModuleTests+MyModuleA", app.ModuleRoot.ChildModules[4].Name);
        Aver.AreEqual("Module4", app.ModuleRoot.ChildModules[5].Name);


        Aver.IsTrue(app.ModuleRoot.ChildModules[0] is MyServiceA);
        Aver.IsTrue(app.ModuleRoot.ChildModules[1] is MyServiceB);
        Aver.IsTrue(app.ModuleRoot.ChildModules[2] is MyModuleA);
        Aver.IsTrue(app.ModuleRoot.ChildModules[3] is MyModuleA);
        Aver.IsTrue(app.ModuleRoot.ChildModules[4] is MyModuleA);
        Aver.IsTrue(app.ModuleRoot.ChildModules[5] is MyModuleB);
      }
    }


    interface IMyLogic : IModule { }
    interface IMyService : IModule { }

    public class MyModuleA : ModuleBase, IMyLogic
    {
      public MyModuleA(IApplication app) : base(app) { }
      public MyModuleA(IModule parent) : base(parent) { }
      public override bool IsHardcodedModule => false;
      public override string ComponentLogTopic => "testing";
    }

    public class MyModuleB : ModuleBase, IMyLogic
    {
      public MyModuleB(IApplication app) : base(app) { }
      public MyModuleB(IModule parent) : base(parent) { }
      public override bool IsHardcodedModule => false;
      public override string ComponentLogTopic => "testing";
    }

    public class MyServiceA : ModuleBase, IMyService
    {
      public MyServiceA(IApplication app) : base(app) { }
      public MyServiceA(IModule parent) : base(parent) { }
      public override bool IsHardcodedModule => false;
      public override string ComponentLogTopic => "testing";
    }

    public class MyServiceB : ModuleBase, IMyService
    {
      public MyServiceB(IApplication app) : base(app) { }
      public MyServiceB(IModule parent) : base(parent) { }
      public override bool IsHardcodedModule => false;
      public override string ComponentLogTopic => "testing";
    }


  }
}