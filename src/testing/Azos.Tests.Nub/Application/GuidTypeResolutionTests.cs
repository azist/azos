/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Scripting;

namespace Azos.Tests.Nub.Application
{
  [Runnable]
  public class GuidTypeResolutionTests
  {
    [Run]
    public void Test_01()
    {
      var got = GuidTypeAttribute.GetGuidTypeAttribute<ClassA, TeztIdAttribute>(typeof(ClassA));
      Aver.IsNotNull(got);
      Aver.IsTrue(got is TeztIdAttribute);
      Aver.AreEqual(Guid.Parse("7554D6D1-B62E-419A-B88D-2B69B51746DC"), got.TypeGuid);
    }

    [Run]
    public void Test_02()
    {
      var got = GuidTypeAttribute.GetGuidTypeAttribute<ClassA, TeztAnotherAttribute>(typeof(ClassA));
      Aver.IsNotNull(got);
      Aver.IsTrue(got is TeztAnotherAttribute);
      Aver.AreEqual(Guid.Parse("AE243F77-25D3-4841-BEBE-4C8CE0B364E4"), got.TypeGuid);
    }

    [Run, Aver.Throws(typeof(AzosException), Message = "attribute in its declaration")]
    public void Test_03()
    {
      GuidTypeAttribute.GetGuidTypeAttribute<ClassB, TeztAnotherAttribute>(typeof(ClassB));//throws
    }

    [Run]
    public void Test_04()
    {
      var got = GuidTypeAttribute.GetGuidTypeAttribute<ClassB, TeztIdAttribute>(typeof(ClassB));
      Aver.IsNotNull(got);
      Aver.IsTrue(got is TeztIdAttribute);
      Aver.AreEqual(Guid.Parse("AE243F77-25D3-4841-BEBE-4C8CE0B364E4"), got.TypeGuid);
    }

    [Run]
    public void Test_05()
    {
      var resolver = new GuidTypeResolver<ClassA, TeztIdAttribute>(typeof(ClassA), typeof(ClassB));
      var got = resolver.Resolve(Guid.Parse("7554D6D1-B62E-419A-B88D-2B69B51746DC"));
      Aver.IsNotNull(got);
      Aver.IsTrue(got == typeof(ClassA));

      got = resolver.Resolve(Guid.Parse("AE243F77-25D3-4841-BEBE-4C8CE0B364E4"));
      Aver.IsNotNull(got);
      Aver.IsTrue(got == typeof(ClassB));
    }

    [Run]
    public void Test_06()
    {
      var resolver = new GuidTypeResolver<ClassA, TeztIdAttribute>("r{ assembly{name='Azos.Tests.Nub.dll' ns='Azos.Tests.Nub.App*'}}".AsLaconicConfig());
      var got = resolver.Resolve(Guid.Parse("7554D6D1-B62E-419A-B88D-2B69B51746DC"));
      Aver.IsNotNull(got);
      Aver.IsTrue(got == typeof(ClassA));

      got = resolver.Resolve(Guid.Parse("AE243F77-25D3-4841-BEBE-4C8CE0B364E4"));
      Aver.IsNotNull(got);
      Aver.IsTrue(got == typeof(ClassB));
    }

    [Run]
    public void Test_07()
    {
      var resolver = new GuidTypeResolver<ClassA, TeztAnotherAttribute>("r{ assembly{name='Azos.Tests.Nub.dll'}}".AsLaconicConfig());
      var got = resolver.Resolve(Guid.Parse("AE243F77-25D3-4841-BEBE-4C8CE0B364E4"));
      Aver.IsNotNull(got);
      Aver.IsTrue(got == typeof(ClassA));

      got = resolver.Resolve(Guid.Parse("C3653758-B881-4C9A-A357-C0537CAFEC5C"));
      Aver.IsNotNull(got);
      Aver.IsTrue(got == typeof(ClassA2));
    }

    [Run]
    public void Test_08()
    {
      var resolver = new GuidTypeResolver<ClassA, TeztAnotherAttribute>("r{ assembly{name='Azos.Tests.Nub.dll'}}".AsLaconicConfig());
      var got = resolver.TryResolve(Guid.Parse("7554D6D1-B62E-419A-B88D-2B69B51746DC"));//guid form a different attribute type
      Aver.IsNull(got);
    }

    [Run, Aver.Throws(typeof(AzosException), Message = "does not map to any")]
    public void Test_09()
    {
      var resolver = new GuidTypeResolver<ClassA, TeztAnotherAttribute>("r{ assembly{name='Azos.Tests.Nub.dll'}}".AsLaconicConfig());
      resolver.Resolve(Guid.Parse("7554D6D1-B62E-419A-B88D-2B69B51746DC"));//THROWS
    }

    public class TeztIdAttribute : GuidTypeAttribute
    {
      public TeztIdAttribute(string s) : base(s){ }
    }

    public class TeztAnotherAttribute : GuidTypeAttribute
    {
      public TeztAnotherAttribute(string s) : base(s) { }
    }

    [TeztId("7554D6D1-B62E-419A-B88D-2B69B51746DC")]
    [TeztAnother("AE243F77-25D3-4841-BEBE-4C8CE0B364E4")]//guid is the same as ClassB.TeztId
    public class ClassA
    {

    }

    [TeztId("AE243F77-25D3-4841-BEBE-4C8CE0B364E4")]
    public class ClassB : ClassA
    {

    }

    [TeztAnother("C3653758-B881-4C9A-A357-C0537CAFEC5C")]
    public class ClassA2 : ClassA
    {

    }

  }
}
