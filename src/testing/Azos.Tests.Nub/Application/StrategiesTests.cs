/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System.Collections.Generic;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Apps.Strategies;
using Azos.Conf;
using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub.Application
{
  [Runnable]
  public class StrategiesTests
  {
    static readonly ConfigSectionNode BASE_CONF = @"
  app{
    modules
    {
      module
      {
        type='Azos.Apps.Strategies.DefaultBinder, Azos'
        assemblies='Azos.Tests.Nub.dll'
      }
    }
  }
  ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);


    [Run]
    public void TestBasicFunctionality()
    {
      using( var app = new AzosApplication(null, BASE_CONF))
      {
        var binder = app.ModuleRoot.Get<IStrategyBinder>();

        var ctx = new MyStartContext{  Value = "val1"};
        var s1 = binder.Bind<IMyStrat1, IMyStratContext>(ctx);

        Aver.IsNotNull(s1);
        Aver.IsTrue(s1 is IMyStrat1);
        Aver.IsTrue(s1 is MyStrat1Impl);
        Aver.AreEqual("MyStrat1Impl.val1", s1.Something());

        var s2 = binder.Bind<IMyStrat2, IMyStratContext>(ctx);

        Aver.IsNotNull(s2);
        Aver.IsTrue(s2 is IMyStrat2);
        Aver.IsTrue(s2 is MyStrat2Impl);
        Aver.AreEqual("SomethingElse MyStrat2Impl.val1", s2.SomethingElse());

        ctx.Value = "gaga";
        Aver.AreEqual("SomethingElse MyStrat2Impl.gaga", s2.SomethingElse());
      }
    }

    interface IMyStratContext : IStrategyContext
    {
      string Value{ get; set;}
    }

    public class MyStartContext : IMyStratContext
    {
      public string Value{ get; set;}
    }

    interface IMyStrat1 : IStrategy<IMyStratContext>
    {
      string Something();
    }

    interface IMyStrat2 : IStrategy<IMyStratContext>
    {
      string SomethingElse();
    }


    class MyStrat1Impl : Strategy<IMyStratContext>, IMyStrat1
    {
      public string Something()
      {
        return "{0}.{1}".Args(GetType().Name, Context.Value);
      }
    }

    class MyStrat2Impl : Strategy<IMyStratContext>, IMyStrat2
    {
    #pragma warning disable 0649
      [Inject] IStrategyBinder m_Binder;//dependency
    #pragma warning restore 0649

      public string SomethingElse()
      {
        Aver.IsNotNull(m_Binder, "DI failed"); //test DI
        return "SomethingElse {0}.{1}".Args(GetType().Name, Context.Value);
      }
    }


  }
}