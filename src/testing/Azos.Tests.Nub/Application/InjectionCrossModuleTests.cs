/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Scripting;

namespace Azos.Tests.Nub.Application
{
  [Runnable]
  public class InjectionCrossModulesTests
  {
    static readonly ConfigSectionNode BASE_CONF = @"
  app{
    modules
    {
      module{type='Azos.Tests.Nub.Application.InjectionCrossModulesTests+ModuleA, Azos.Tests.Nub' }
      module{type='Azos.Tests.Nub.Application.InjectionCrossModulesTests+ModuleB, Azos.Tests.Nub' }
    }
  }
  ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);


    [Run]
    public void Test_1()
    {
      using( var app = new AzosApplication(null, BASE_CONF))
      {
        var moduleA = app.ModuleRoot.Get<IModuleA>();
        moduleA.TestA();
      }
    }

    [Run]
    public void Test_2()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var moduleB = app.ModuleRoot.Get<IModuleB>();
        moduleB.TestB();
      }
    }

    [Run]
    public void Test_3()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var moduleA = app.ModuleRoot.Get<IModuleA>();
        var moduleB = app.ModuleRoot.Get<IModuleB>();
        moduleA.TestA();
        moduleB.TestB();
      }
    }

    [Run]
    public void Test_4()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var moduleB = app.ModuleRoot.Get<IModuleB>();
        var valueB = moduleB.ValueB;

        Aver.AreEqual("value456789", valueB);
      }
    }


    interface IModuleA : IModule
    {
      int ValueA {  get; }
      void TestA();
    }

    interface IModuleB : IModule
    {
      string ValueB {  get; }
      void TestB();
    }


    public class ModuleA : ModuleBase, IModuleA
    {
      [Inject] IModuleA m_ModuleA;
      [Inject] IModuleB m_ModuleB;

      public ModuleA(IApplication app) : base(app) { }
      public ModuleA(IModule parent) : base(parent) { }
      public override bool IsHardcodedModule => false;
      public override string ComponentLogTopic => "testing";

      public int ValueA => 456789;

      public void TestA()
      {
        Aver.AreSameRef(this, m_ModuleA);
        Aver.AreEqual("value456789", m_ModuleB.ValueB);
      }
    }

    public class ModuleB : ModuleBase, IModuleB
    {
      [Inject] IModuleA m_ModuleA;
      [Inject] IModuleB m_ModuleB;

      public ModuleB(IApplication app) : base(app) { }
      public ModuleB(IModule parent) : base(parent) { }
      public override bool IsHardcodedModule => false;
      public override string ComponentLogTopic => "testing";

      public string ValueB => "value{0}".Args(m_ModuleA.ValueA);

      public void TestB()
      {
        Aver.AreSameRef(this, m_ModuleB);
        Aver.AreEqual("value456789", this.ValueB);
      }
    }

  }
}