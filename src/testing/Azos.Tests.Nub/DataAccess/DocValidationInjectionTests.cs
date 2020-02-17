using System;
using System.Collections.Generic;
using System.Text;


using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Conf;
using Azos.Data;
using Azos.Scripting;

#pragma warning disable 649
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

      public override ValidState Validate(ValidState state)
      {
        Aver.IsNotNull(m_App);
        Aver.IsNotNull(m_Module);
        return base.Validate(state);
      }
    }

    public class DocCompositeField : TypedDoc
    {
      [Inject] ISpecialModule m_Module;


      [Field] public int? I1 { get; set; }
      [Field] public DocDirectField D1 { get; set; }
      [Field] public DocDirectField D2 { get; set; }

      public override ValidState Validate(ValidState state)
      {
        Aver.IsNotNull(m_Module);
        return base.Validate(state);
      }
    }

    public class DocArray : TypedDoc
    {
      [Inject] ISpecialModule m_Module;


      [Field(min: 100)] public int? IMin { get; set; }
      [Field] public DocCompositeField[] DArray { get; set; }

      public override ValidState Validate(ValidState state)
      {
        Aver.IsNotNull(m_Module);
        return base.Validate(state);
      }
    }

    public class DocList : TypedDoc
    {
      [Inject] ISpecialModule m_Module;


      [Field(min: 100)] public int? IMin { get; set; }
      [Field] public List<object> DList { get; set; }

      public override ValidState Validate(ValidState state)
      {
        Aver.IsNotNull(m_Module);
        return base.Validate(state);
      }
    }


    public class DocDict : TypedDoc
    {
      [Inject] ISpecialModule m_Module;


      [Field(min: 100)] public int? IMin { get; set; }
      [Field] public Dictionary<string, DocCompositeField> DDict { get; set; }

      public override ValidState Validate(ValidState state)
      {
        Aver.IsNotNull(m_Module);
        return base.Validate(state);
      }
    }

   //==================================================================================================

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


    [Run]
    public void Test_DocArray_1()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocArray();
        doc.IMin = 1000;
        doc.DArray = new[]{ new DocCompositeField{ D1 = new DocDirectField{ S1="key"}, D2 = new DocDirectField { S1 = "key" } } };
        Aver.IsNull(doc.Validate(app));
      }
    }

    [Run]
    public void Test_DocArray_2()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocArray();
        doc.IMin = 5;//below acceptable
        doc.DArray = new[] { new DocCompositeField { D1 = new DocDirectField { S1 = "key" }, D2 = new DocDirectField { S1 = "key" } } };

        var ve = doc.Validate(app);
        Aver.IsNotNull(ve);
        if (ve is FieldValidationException fve)
        {
          Console.WriteLine(fve.Message);
          Aver.IsTrue(fve.Message.Contains("min"));
        }
        else
          Aver.Fail("Not a FVExcp");


        doc.IMin= 5000;
        ve = doc.Validate(app);
        Aver.IsNull(ve);
      }
    }

    [Run]
    public void Test_DocArray_3()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocArray();
        doc.IMin = 5000;
        doc.DArray = new[] { new DocCompositeField { D1 = new DocDirectField { S1 = "zzzzkey" }, D2 = new DocDirectField { S1 = "key" } } };

        var ve = doc.Validate(app);
        Aver.IsNotNull(ve);
        if (ve is FieldValidationException fve)
        {
          Console.WriteLine(fve.Message);
          Aver.IsTrue(fve.Message.Contains("list"));
        }
        else
          Aver.Fail("Not a FVExcp");


        doc.DArray[0].D1.S1="key";
        ve = doc.Validate(app);
        Aver.IsNull(ve);
      }
    }


    [Run]
    public void Test_DocArray_4()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocArray();
        doc.IMin = 1000;
        doc.DArray = new[] { null, null, null, new DocCompositeField { D1 = new DocDirectField { S1 = "key" }, D2 = new DocDirectField { S1 = "key" } } };
        Aver.IsNull(doc.Validate(app));
      }
    }




    [Run]
    public void Test_DocList_1()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocList();
        doc.IMin = 1000;
        doc.DList = new List<object> { new DocCompositeField { D1 = new DocDirectField { S1 = "key" }, D2 = new DocDirectField { S1 = "key" } } };
        Aver.IsNull(doc.Validate(app));
      }
    }

    [Run]
    public void Test_DocList_2()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocList();
        doc.IMin = 5;//below acceptable
        doc.DList = new List<object> { new DocCompositeField { D1 = new DocDirectField { S1 = "key" }, D2 = new DocDirectField { S1 = "key" } } };

        var ve = doc.Validate(app);
        Aver.IsNotNull(ve);
        if (ve is FieldValidationException fve)
        {
          Console.WriteLine(fve.Message);
          Aver.IsTrue(fve.Message.Contains("min"));
        }
        else
          Aver.Fail("Not a FVExcp");


        doc.IMin = 5000;
        ve = doc.Validate(app);
        Aver.IsNull(ve);
      }
    }

    [Run]
    public void Test_DocList_3()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocList();
        doc.IMin = 5000;
        doc.DList = new List<object> { new DocCompositeField { D1 = new DocDirectField { S1 = "zzzzkey" }, D2 = new DocDirectField { S1 = "key" } } };

        var ve = doc.Validate(app);
        Aver.IsNotNull(ve);
        if (ve is FieldValidationException fve)
        {
          Console.WriteLine(fve.Message);
          Aver.IsTrue(fve.Message.Contains("list"));
        }
        else
          Aver.Fail("Not a FVExcp");


        ((DocCompositeField)doc.DList[0]).D1.S1 = "key";
        ve = doc.Validate(app);
        Aver.IsNull(ve);
      }
    }


    [Run]
    public void Test_DocList_4()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocList();
        doc.IMin = 1000;
        doc.DList = new List<object> { null, null, null, new DocCompositeField { D1 = new DocDirectField { S1 = "key" }, D2 = new DocDirectField { S1 = "key" } } };
        Aver.IsNull(doc.Validate(app));
      }
    }




    [Run]
    public void Test_DocDict_1()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocDict();
        doc.IMin = 1000;
        doc.DDict = new Dictionary<string, DocCompositeField> { {"a",new DocCompositeField { D1 = new DocDirectField { S1 = "key" }}}, {"b", new DocCompositeField { D1 = new DocDirectField { S1 = "key" }}}};
        Aver.IsNull(doc.Validate(app));
      }
    }

    [Run]
    public void Test_DocDict_2()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocDict();
        doc.IMin = -100;
        doc.DDict = new Dictionary<string, DocCompositeField> { { "a", new DocCompositeField { D1 = new DocDirectField { S1 = "key" } } }, { "b", new DocCompositeField { D1 = new DocDirectField { S1 = "key" } } } };

        var ve = doc.Validate(app);
        Aver.IsNotNull(ve);
        if (ve is FieldValidationException fve)
        {
          Console.WriteLine(fve.Message);
          Aver.IsTrue(fve.Message.Contains("min"));
        }
        else
          Aver.Fail("Not a FVExcp");


        doc.IMin = 5000;
        ve = doc.Validate(app);
        Aver.IsNull(ve);
      }
    }

    [Run]
    public void Test_DocDict_3()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocDict();
        doc.IMin = 1000;
        doc.DDict = new Dictionary<string, DocCompositeField> { { "a", new DocCompositeField { D1 = new DocDirectField { S1 = "kwerweey" } } }, { "b", new DocCompositeField { D1 = new DocDirectField { S1 = "key" } } } };


        var ve = doc.Validate(app);
        Aver.IsNotNull(ve);
        if (ve is FieldValidationException fve)
        {
          Console.WriteLine(fve.Message);
          Aver.IsTrue(fve.Message.Contains("list"));
        }
        else
          Aver.Fail("Not a FVExcp");


        doc.DDict["a"].D1.S1 = "key";
        ve = doc.Validate(app);
        Aver.IsNull(ve);
      }
    }

    [Run]
    public void Test_DocDict_4()
    {
      using (var app = new AzosApplication(null, BASE_CONF))
      {
        var doc = new DocDict();
        doc.IMin = 1000;
        doc.DDict = new Dictionary<string, DocCompositeField> { {"x",null}, {"y",null}, { "a", new DocCompositeField { D1 = new DocDirectField { S1 = "key" } } }, { "b", new DocCompositeField { D1 = new DocDirectField { S1 = "key" } } } };
        Aver.IsNull(doc.Validate(app));
      }
    }



  }
#pragma warning restore 649
}
