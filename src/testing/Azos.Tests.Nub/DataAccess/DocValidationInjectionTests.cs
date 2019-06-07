using System;
using System.Collections.Generic;
using System.Text;


using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class DocValidationInjectionTests
  {
    interface ISpecialModule : IModule { }

    public class SpecialModule : ModuleBase, ISpecialModule
    {
      public SpecialModule(IApplication app) : base(app) { }
      public SpecialModule(IModule parent) : base(parent) { }
      public override bool IsHardcodedModule => false;
      public override string ComponentLogTopic => "testing";
    }


    static readonly ConfigSectionNode BASE_CONF = @"
    app{
      modules
      {
        module{type='Azos.Tests.Nub.DataAccess.DocValidationInjectionTests+SpecialModule, Azos.Tests.Nub' }
      }
    }
    ".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);


    public class DocDirectField : TypedDoc
    {
      [Inject] IApplication m_App;
      [Inject] ISpecialModule m_Module;

      [Field(valueList: "key:val")] public string S1{ get;set;}

      public override Exception Validate(string targetName)
      {
        Aver.IsNotNull(m_App);
        Aver.IsNotNull(m_Module);
        return base.Validate(targetName);
      }
    }

    public class DocCompositeField : TypedDoc
    {
      [Inject] ISpecialModule m_Module;


      [Field] public int? I1 { get; set; }
      [Field] public DocDirectField D1 { get; set; }
      [Field] public DocDirectField D2 { get; set; }

      public override Exception Validate(string targetName)
      {
        Aver.IsNotNull(m_Module);
        return base.Validate(targetName);
      }
    }



    [Run]
    public void Test_DocDirectField_1()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocDirectField();
        Aver.IsNull( doc.Validate(app) );
      }
    }

    [Run]
    public void Test_DocDirectField_2()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocDirectField();
        doc.S1 = "a";
        var ve = doc.Validate(app);
        Aver.IsNotNull(ve);
        if (ve is FieldValidationException fve)
        {
          Console.WriteLine(fve.Message);
          Aver.IsTrue( fve.Message.Contains("list"));
        }
        else
          Aver.Fail("Not a FVExcp");


        doc.S1 = "key";
        ve = doc.Validate(app);
        Aver.IsNull(ve);
      }
    }

    [Run]
    public void Test_DocCompositeField_1()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocCompositeField();
        doc.D1 = new DocDirectField();
        doc.D2 = new DocDirectField{ S1="key"};
        Aver.IsNull(doc.Validate(app));
      }
    }

    [Run]
    public void Test_DocCompositeField_2()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocCompositeField();
        doc.D1 = new DocDirectField();
        doc.D2 = new DocDirectField { S1 = "kgergegergegegeyrepkpperunvpuewpfoue[rputme[wutmpwempfouwm" };
        var ve = doc.Validate(app);
        Aver.IsNotNull(ve);
        if (ve is FieldValidationException fve)
        {
          Console.WriteLine(fve.Message);
          Aver.IsTrue(fve.Message.Contains("list"));
        }
        else
          Aver.Fail("Not a FVExcp");


        doc.D2.S1 = "key";
        ve = doc.Validate(app);
        Aver.IsNull(ve);
      }
    }



  }
}
