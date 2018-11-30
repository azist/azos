/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Scripting;

namespace Azos.Tests.Nub.Application
{
  [Runnable]
  public class ModuleTests
  {
    static readonly ConfigSectionNode BASE_CONF = @"
  app{
    modules
    {
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyModuleA, Azos.Tests.Nub' order=3 key = 3333}
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyModuleA, Azos.Tests.Nub' name='Module1' order=1 key=1000}
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyModuleB, Azos.Tests.Nub' name='Module4' order=4 key=4000}
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyModuleA, Azos.Tests.Nub' name='Module2' order=2 key= 2200 }
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyServiceA, Azos.Tests.Nub' name = 's1'}
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyServiceB, Azos.Tests.Nub' name = 's2'}
    }
  }
  ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

    static readonly ConfigSectionNode DUPLICATE_CONF1 = @"
  app{
    modules
    {
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyModuleA, Azos.Tests.Nub'}
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyModuleA, Azos.Tests.Nub'}
    }
  }
  ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

    static readonly ConfigSectionNode DUPLICATE_CONF2 = @"
  app{
    modules
    {
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyModuleA, Azos.Tests.Nub' name='module1'}
      module{type='Azos.Tests.Nub.Application.ModuleTests+MyModuleB, Azos.Tests.Nub' name='module1'}
    }
  }
  ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);



    [Run]
    public void Test_ModuleInjectionAndOrdering()
    {
      using( var app = new AzosApplication(null, BASE_CONF))
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

    [Run]
    public void Test_ModuleInjectionAndAccess()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        Aver.AreEqual(6, app.ModuleRoot.ChildModules.Count);

        var logic = app.ModuleRoot.TryGet<IMyLogic>("Module1");
        Aver.IsNotNull(logic);
        Aver.AreEqual(1000, logic.Key);

        logic = app.ModuleRoot.TryGet<IMyLogic>("Module2");
        Aver.IsNotNull(logic);
        Aver.AreEqual(2200, logic.Key);

        logic = app.ModuleRoot.TryGet<IMyLogic>("Module3");
        Aver.IsNull(logic);

        logic = app.ModuleRoot.TryGet<IMyLogic>("Module4");
        Aver.IsNotNull(logic);
        Aver.AreEqual(4000, logic.Key);

        logic = app.ModuleRoot.TryGet<IMyLogic>("s1");
        Aver.IsNull(logic);

        Aver.Throws<AzosException>(() => app.ModuleRoot.Get<IMyLogic>("s1") );

        var svc = app.ModuleRoot.TryGet<IMyService>("s1");
        Aver.IsNotNull(svc);
        Aver.IsTrue( svc is MyServiceA);

        svc = app.ModuleRoot.TryGet<IMyService>("s2");
        Aver.IsNotNull(svc);
        Aver.IsTrue(svc is MyServiceB);

      }
    }

    [Run]
    [Aver.Throws(typeof(AzosException), Message = "module already contains a child")]
    public void Test_DuplicateModule1()
    {
      using (var app = new AzosApplication(null, DUPLICATE_CONF1)) { }
    }

    [Run]
    [Aver.Throws(typeof(AzosException), Message = "module already contains a child")]
    public void Test_DuplicateModule2()
    {
      using (var app = new AzosApplication(null, DUPLICATE_CONF2)) { }
    }


    interface IMyLogic : IModule { int Key { get; set; } }
    interface IMyService : IModule { }

    public class MyModuleA : ModuleBase, IMyLogic
    {
      public MyModuleA(IApplication app) : base(app) { }
      public MyModuleA(IModule parent) : base(parent) { }
      public override bool IsHardcodedModule => false;
      public override string ComponentLogTopic => "testing";

      [Config]public int Key {  get; set; }
    }

    public class MyModuleB : ModuleBase, IMyLogic
    {
      public MyModuleB(IApplication app) : base(app) { }
      public MyModuleB(IModule parent) : base(parent) { }
      public override bool IsHardcodedModule => false;
      public override string ComponentLogTopic => "testing";

      [Config] public int Key { get; set; }
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