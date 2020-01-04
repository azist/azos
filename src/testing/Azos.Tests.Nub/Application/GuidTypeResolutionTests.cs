using System;
using System.Collections.Generic;
using System.Text;

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
    public class ClassB
    {

    }

    [TeztAnother("{C3653758-B881-4C9A-A357-C0537CAFEC5C}")]
    public class ClassA2 : ClassA
    {

    }

  }
}
