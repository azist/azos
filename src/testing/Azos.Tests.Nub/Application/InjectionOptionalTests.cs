/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Scripting;
using Azos.Tests.Nub;

namespace Azos.Tests.Nub.Application
{
  [Runnable]
  public class InjectionOptionalTests
  {
    static readonly ConfigSectionNode BASE_CONF = @"
  app{
    modules
    {
      module{type='Azos.Tests.Nub.Application.InjectionOptionalTests+ModuleA, Azos.Tests.Nub' }
      module{type='Azos.Tests.Nub.Application.InjectionOptionalTests+ModuleB, Azos.Tests.Nub' }
    }
  }
  ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

    static readonly ConfigSectionNode BAD_CONF = @"
  app{
    modules
    {
      module{type='Azos.Tests.Nub.Application.InjectionOptionalTests+ModuleA, Azos.Tests.Nub' }
      //B is missing
    }
  }
  ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);


    [Run]
    public void Test_1()
    {
      using( var app = new AzosApplication(null, BASE_CONF))
      {
        var moduleA = app.ModuleRoot.Get<IModuleA>();
        var moduleB = app.ModuleRoot.Get<IModuleB>();
        Aver.AreEqual(100, moduleA.ValueOfA);
        Aver.AreEqual(-1000, moduleB.ValueOfB);
      }
    }

    [Run]
    public void Test_2()
    {
      using (var app = new AzosApplication(null, BAD_CONF))
      {
        var moduleA = app.ModuleRoot.Get<IModuleA>();
        Aver.IsNotNull(moduleA);

        var moduleB = app.ModuleRoot.TryGet<IModuleB>();
        Aver.IsNull(moduleB);

        try
        {
          app.ModuleRoot.Get<IModuleB>();
        }
        catch(AzosException e)
        {
          new WrappedExceptionData(e).See();
          return;
        }

        Aver.Fail(Constants.ERR_NOT_THROWN);
      }
    }

    [Run]
    public void Test_3()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var obj = new NeedsBoth();
        app.InjectInto(obj);
        Aver.AreEqual(100, obj.A.ValueOfA);
        Aver.AreEqual(-1000, obj.B.ValueOfB);
      }
    }

    [Run]
    public void Test_4()
    {
      using (var app = new AzosApplication(null, BAD_CONF))
      {
        var obj = new NeedsBoth();
        try
        {
          app.InjectInto(obj);
        }
        catch (DependencyInjectionException e)
        {
          new WrappedExceptionData(e).See();
          return;
        }

        Aver.Fail(Constants.ERR_NOT_THROWN); //because NeedsBoth requires A and B
      }
    }

    [Run]
    public void Test_5()
    {
      using (var app = new AzosApplication(null, BAD_CONF))
      {
        var obj = new NeedsOne();//This only requires A but not B
        app.InjectInto(obj);
        Aver.AreEqual(100, obj.A.ValueOfA);
        Aver.IsNull(obj.B);//OPTIONAL dependency
      }
    }

    [Run]
    public void Test_6()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var obj = new NeedsOne();//This only requires A but not B, but if B is there it will inject it
        app.InjectInto(obj);
        Aver.AreEqual(100, obj.A.ValueOfA);
        Aver.IsNotNull(obj.B);//OPTIONAL dependency IS injected WHEN available
        Aver.AreEqual(-1000, obj.B.ValueOfB);
      }
    }


    public class NeedsBoth
    {
      [Inject] public IModuleA A;
      [Inject] public IModuleB B;
    }

    public class NeedsOne
    {
      [Inject] public IModuleA A;
      [Inject(Optional = true)] public IModuleB B;
    }


    public interface IModuleA : IModule
    {
      int ValueOfA {  get; }
    }

    public interface IModuleB : IModule
    {
      int ValueOfB {  get; }
    }


    public class ModuleA : ModuleBase, IModuleA
    {
      public ModuleA(IApplication app) : base(app) { }
      public ModuleA(IModule parent) : base(parent) { }
      public override bool IsHardcodedModule => false;
      public override string ComponentLogTopic => "testing";

      public int ValueOfA => 100;
    }

    public class ModuleB : ModuleBase, IModuleB
    {
      public ModuleB(IApplication app) : base(app) { }
      public ModuleB(IModule parent) : base(parent) { }
      public override bool IsHardcodedModule => false;
      public override string ComponentLogTopic => "testing";

      public int ValueOfB => -1000;
    }

  }
}