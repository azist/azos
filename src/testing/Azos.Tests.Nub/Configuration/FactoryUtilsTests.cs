/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class FactoryUtilsTests
  {
    [Run]
    public void Make_1()
    {
      var cfg = " type='Azos.Tests.Nub.Configuration.ClassA, Azos.Tests.Nub' ".AsLaconicConfig();
      var got = FactoryUtils.Make<IA>(cfg);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassA);
    }

    [Run]
    public void Make_2()
    {
      var cfg = " type='Azos.Tests.Nub.Configuration.ClassA, Azos.Tests.Nub' ".AsLaconicConfig();
      var got = FactoryUtils.Make<ClassA>(cfg);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassA);
    }

    [Run]
    public void Make_3()
    {
      var cfg = " type='Azos.Tests.Nub.Configuration.ClassAB, Azos.Tests.Nub' ".AsLaconicConfig();
      var got = FactoryUtils.Make<ClassA>(cfg);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassAB);
    }

    [Run, Aver.Throws(typeof(ConfigException), Message = "not assignable")]
    public void Make_4()
    {
      var cfg = " type='Azos.Tests.Nub.Configuration.ClassA, Azos.Tests.Nub' ".AsLaconicConfig();
      var got = FactoryUtils.Make<ClassAB>(cfg);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassAB);
    }

    [Run]
    public void Make_5()
    {
      var cfg = " type='Azos.Tests.Nub.Configuration.ClassAB, Azos.Tests.Nub' ".AsLaconicConfig();
      var got = FactoryUtils.Make<ClassAB>(cfg);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassAB);
    }

    [Run]
    public void Make_6()
    {
      var cfg = "a{ type-path='Azos.Tests.Nub.Configuration, Azos.Tests.Nub'  type=ClassAB  }".AsLaconicConfig();
      var got = FactoryUtils.Make<ClassAB>(cfg);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassAB);
    }

    [Run]
    public void Make_6_01()
    {
      var cfg = "a{ type=ClassAB  }".AsLaconicConfig();
      cfg.TypeSearchPaths = new[]{ "Azos.Tests.Nub.Configuration, Azos.Tests.Nub" };
      var got = FactoryUtils.Make<ClassAB>(cfg);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassAB);
    }

    [Run]
    public void Make_6_02()
    {
      var cfg = "a{ type-path='System.Collections'  type=ClassAB  }".AsLaconicConfig();
      cfg.TypeSearchPaths = new[] { "Azos.Tests.Nub.Configuration, Azos.Tests.Nub" };
      var got = FactoryUtils.Make<ClassAB>(cfg);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassAB);
    }

    [Run]
    public void Make_7()
    {
      var cfg = "a{ type-path='Azos.Tests.Nub.Configuration, Azos.Tests.Nub' b{ type=ClassAB } }".AsLaconicConfig();
      var got = FactoryUtils.Make<ClassAB>(cfg["b"]);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassAB);
    }

    [Run]
    public void Make_7_01()
    {
      var cfg = "a{  b{ type=ClassAB }  }".AsLaconicConfig();
      cfg.TypeSearchPaths = new[] { "Azos.Tests.Nub.Configuration, Azos.Tests.Nub" };
      var got = FactoryUtils.Make<ClassAB>(cfg["b"]);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassAB);
    }

    [Run]
    public void Make_8()
    {
      var cfg = "a{ type-path='Azos.Tests.Nub.Configuration, Azos.Tests.Nub' b{ c{ type=ClassAB } } }".AsLaconicConfig();
      var got = FactoryUtils.Make<ClassAB>(cfg["b"]["c"]);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassAB);
    }

    [Run, Aver.Throws(typeof(ConfigException), Message = "could not be created")]
    public void Make_9()
    {
      var cfg = "a{ type-path='Azos.Tests.Nub.Configuration, Azos.Tests.Nub' b{ type-path=null c{ type=ClassAB } } }".AsLaconicConfig();
      var got = FactoryUtils.Make<ClassAB>(cfg["b"]["c"]);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassAB);
    }

    [Run]
    public void Make_10()
    {
      var cfg = "a{ type-path='System.Text, System; Azos.Tests.Nub.Configuration, Azos.Tests.Nub' b{ c{ type=ClassAB } } }".AsLaconicConfig();
      var got = FactoryUtils.Make<ClassAB>(cfg["b"]["c"]);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassAB);
    }

    [Run, Aver.Throws(typeof(ConfigException), Message = "could not be created")]
    public void Make_11()
    {
      var cfg = "a{ type-path='Azos.Tests.Nub.Configuration, Azos.Tests.Nub' b{ type-path='System.Text,System' c{ type=ClassAB } } }".AsLaconicConfig();
      var got = FactoryUtils.Make<ClassAB>(cfg["b"]["c"]);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassAB);
    }

    [Run]
    public void Make_12()
    {
      var cfg = "a{ }".AsLaconicConfig();
      var got = FactoryUtils.Make<ClassAB>(cfg["b"]["c"], typeof(ClassAB));
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassAB);
    }


    [Run]
    public void MakeAndConfigure_1()
    {
      var cfg = " type='Azos.Tests.Nub.Configuration.ClassB, Azos.Tests.Nub' a=7 b='Jack London' ".AsLaconicConfig();
      var got = FactoryUtils.MakeAndConfigure<ClassB>(cfg);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassB);
      Aver.AreEqual(7, got.A);
      Aver.AreEqual("Jack London", got.B);
    }

    [Run]
    public void MakeAndConfigure_2()
    {
      var cfg = " type='Azos.Tests.Nub.Configuration.ClassBB, Azos.Tests.Nub' a=7 b='Theodore Dreiser'  c=12.67".AsLaconicConfig();
      var got = FactoryUtils.MakeAndConfigure<ClassBB>(cfg);
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassBB);
      Aver.AreEqual(7, got.A);
      Aver.AreEqual("Theodore Dreiser", got.B);
      Aver.AreEqual(12.67, got.C);
    }

    [Run]
    public void MakeAndConfigure_3()
    {
      var cfg = " type='Azos.Tests.Nub.Configuration.ClassBB, Azos.Tests.Nub' a=7 b='Theodore Dreiser'  c=12.67".AsLaconicConfig();
      var got = FactoryUtils.MakeAndConfigure<ClassBB>(cfg, args: new[] { "Gershwin" });
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassBB);
      Aver.AreEqual(7, got.A);
      Aver.AreEqual("Theodore Dreiser", got.B);
      Aver.AreEqual(12.67, got.C);
      Aver.AreEqual("Gershwin", got.Name);
    }

    [Run]
    public void MakeAndConfigure_4()
    {
      var cfg = " type='Azos.Tests.Nub.Configuration.ClassBB, Azos.Tests.Nub' a=7 b='Theodore Dreiser'  c=12.67".AsLaconicConfig();
      var got = FactoryUtils.MakeAndConfigure<ClassBB>(cfg, typeof(ClassB), args: new[] { "Gershwin" });
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassBB);
      Aver.AreEqual(7, got.A);
      Aver.AreEqual("Theodore Dreiser", got.B);
      Aver.AreEqual(12.67, got.C);
      Aver.AreEqual("Gershwin", got.Name);
    }

    [Run]
    public void MakeAndConfigure_5()
    {
      var cfg = "  a=7 b='Theodore Dreiser'  c=12.67".AsLaconicConfig();
      var got = FactoryUtils.MakeAndConfigure<ClassBB>(cfg, typeof(ClassBB), args: new[] { "Gershwin" });
      Aver.IsNotNull(got);
      Aver.IsTrue(got is ClassBB);
      Aver.AreEqual(7, got.A);
      Aver.AreEqual("Theodore Dreiser", got.B);
      Aver.AreEqual(12.67, got.C);
      Aver.AreEqual("Gershwin", got.Name);
    }



    [Run]
    public void MakeUsingCtor_1()
    {
      var made = FactoryUtils.MakeUsingCtor<CTORClass>("node{arg0=1 arg1=2}".AsLaconicConfig());
      Aver.AreEqual(1, made.A);
      Aver.AreEqual(2, made.B);
    }

    [Run]
    public void MakeUsingCtor_2()
    {
      var made = FactoryUtils.MakeUsingCtor<CTORClass>("node{arg0='7/1/1982'}".AsLaconicConfig());
      Aver.AreEqual(1982, made.A);
      Aver.AreEqual(7, made.B);
    }

    [Run]
    public void MakeUsingCtor_3()
    {
      var made = FactoryUtils.MakeUsingCtor<CTORClass>("node{type='Azos.Tests.Nub.Configuration.CTORClassDerived, Azos.Tests.Nub' arg0=1 arg1=2}".AsLaconicConfig());

      Aver.IsTrue(made is CTORClassDerived);
      Aver.AreEqual(1, made.A);
      Aver.AreEqual(2, made.B);
    }

    [Run]
    public void MakeUsingCtor_4()
    {
      var made = FactoryUtils.MakeUsingCtor<CTORClass>("node{type='Azos.Tests.Nub.Configuration.CTORClassDerived, Azos.Tests.Nub' arg0='7/1/1982'}".AsLaconicConfig());

      Aver.IsTrue(made is CTORClassDerived);
      Aver.AreEqual(1982, made.A);
      Aver.AreEqual(7, made.B);
      Aver.AreEqual(155, ((CTORClassDerived)made).C);
    }


    [Run]
    [Aver.Throws(typeof(ConfigException), Message = "MakeUsingCtor")]
    public void MakeUsingCtor_5()
    {
      var made = FactoryUtils.MakeUsingCtor<CTORClass>("node{arg0=1}".AsLaconicConfig());
    }


    [Run]
    public void MakeUsingCtor_6()
    {
      var made = FactoryUtils.MakeAndConfigure<CTORClass>(
          "node{type='Azos.Tests.Nub.Configuration.CTORClassDerived, Azos.Tests.Nub' data1='AAA' data2='bbb'}".AsLaconicConfig(), args: new object[] { 1, 12 });

      Aver.IsTrue(made is CTORClassDerived);
      Aver.AreEqual(1, made.A);
      Aver.AreEqual(12, made.B);
      Aver.AreEqual("AAA", ((CTORClassDerived)made).Data1);
      Aver.AreEqual("bbb", ((CTORClassDerived)made).Data2);
    }


    [Run]
    public void MakeUsingCtor_7_typePattern()
    {
      var made = FactoryUtils.MakeUsingCtor<CTORClass>(
          "node{type='CTORClassDerived' arg0='12' arg1='234'}".AsLaconicConfig(), typePattern: "Azos.Tests.Nub.Configuration.*, Azos.Tests.Nub");

      Aver.IsTrue(made is CTORClassDerived);
      Aver.AreEqual(12, made.A);
      Aver.AreEqual(234, made.B);
    }


  }


  public interface IA
  {

  }

  public class ClassA : IA
  {

  }

  public class ClassAB : ClassA
  {

  }

  public interface IB : IConfigurable
  {

  }

  public class ClassB : IB
  {
    [Config] public int A;
    [Config] public string B;

    public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, node);
  }

  public class ClassBB : ClassB
  {
    public ClassBB() { }
    public ClassBB(string name) => Name = name;

    public string Name;
    [Config] public double C;
  }




  public class CTORClass : IConfigurable
  {
    public CTORClass(int a, int b) { A = a; B = b; }
    public CTORClass(DateTime dt) { A = dt.Year; B = dt.Month; }
    public readonly int A; public readonly int B;

    public void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }
  }

  public class CTORClassDerived : CTORClass
  {
    public CTORClassDerived(int a, int b) : base(a, b) { }
    public CTORClassDerived(DateTime dt) : base(dt) { }

    public readonly int C = 155;

    [Config]
    public string Data1 { get; set; }

    [Config]
    public string Data2;
  }

}
