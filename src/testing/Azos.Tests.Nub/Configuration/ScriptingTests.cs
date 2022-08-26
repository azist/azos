/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class ScriptingTests
  {

    const string src1 =
@"
root
{
   a=12
   b=true
   _loop='$(/$a) < 15'
   {
        _set{ path=/$a to=$(/$a)+1}
        sectionLoop { name=section_$(/$a) value='something'}
   }

   _call=/sub_Loop {}

   kerosine {}

   _call=/sub_Loop {}

   sub_Loop
   {
        script-only=true
        cnt=0{script-only=true}
         _set{ path=../cnt to=0}
         _loop='$(../cnt) < 5'
         {
                _set{ path=/sub_Loop/cnt to=$(/sub_Loop/cnt)+1}

                _if='$(/sub_Loop/cnt)==3'
                {
                    fromSubLoopFOR_THREE { name=section_$(/sub_Loop/cnt) value='3 gets special handling'}
                }
                _else
                {
                    fromSubLoop { name=section_$(/sub_Loop/cnt) value='something'}
                }
         }

   }

   benzin{}

   c = 45
}//root
";

    const string expected1 =
   @"root
{
  a=12
  b=true
  c=45
  sectionLoop
  {
    name=section_13
    value=something
  }
  sectionLoop
  {
    name=section_14
    value=something
  }
  sectionLoop
  {
    name=section_15
    value=something
  }
  fromSubLoop
  {
    name=section_1
    value=something
  }
  fromSubLoop
  {
    name=section_2
    value=something
  }
  fromSubLoopFOR_THREE
  {
    name=section_3
    value=""3 gets special handling""
  }
  fromSubLoop
  {
    name=section_4
    value=something
  }
  fromSubLoop
  {
    name=section_5
    value=something
  }
  kerosine
  {
  }
  fromSubLoop
  {
    name=section_1
    value=something
  }
  fromSubLoop
  {
    name=section_2
    value=something
  }
  fromSubLoopFOR_THREE
  {
    name=section_3
    value=""3 gets special handling""
  }
  fromSubLoop
  {
    name=section_4
    value=something
  }
  fromSubLoop
  {
    name=section_5
    value=something
  }
  benzin
  {
  }
}";



    [Run]
    public void VarsLoopIfElseCall()
    {
      var src = Azos.Conf.LaconicConfiguration.CreateFromString(src1);
      var result = new Azos.Conf.LaconicConfiguration();

      new ScriptRunner().Execute(src, result);

      var got = result.SaveToString();
      got.See();
      Aver.AreEqual(expected1.ToWindowsLines(), got.ToWindowsLines());
    }

    [Run]
    public void ExprEval1_TernaryIf()
    {
      var src = Azos.Conf.LaconicConfiguration.CreateFromString(
@"
root
{
   a=12
   b=true

   var1=0{script-only=true}
   var2=175.4{script-only=true}
   var3=true{script-only=true}

   _block
   {
       _set{ path=/var1 to=(?$(/var2)>10;15;-10)+100 }
       RESULT=$(/var1){}
   }
}
");
      var result = new Azos.Conf.LaconicConfiguration();

      new ScriptRunner().Execute(src, result);

      var got = result.SaveToString();
      got.See();

      Aver.AreEqual(115, result.Root["RESULT"].ValueAsInt());
    }


    [Run]
    public void ExprEval1_TernaryIfWithMixingTypes()
    {
      var src = Azos.Conf.LaconicConfiguration.CreateFromString(
@"
root
{
   a=12
   b=true

   var1=0{script-only=true}
   var2=175.4{script-only=true}
   var3=true{script-only=true}

   _block
   {
       _set{ path=/var1 to='((?$(/var3);$(/var2);-10)+100)+kozel' }
       RESULT=$(/var1){}
   }
}
");
      var result = new Azos.Conf.LaconicConfiguration();

      new ScriptRunner().Execute(src, result);

      var got = result.SaveToString();
      got.See();

      Aver.AreEqual("275.4kozel", result.Root["RESULT"].Value);
    }



    [Run]
    [Aver.Throws(typeof(ConfigException), Message = "which does not exist")]
    public void Error_SetVarDoesNotExist()
    {
      var src = Azos.Conf.LaconicConfiguration.CreateFromString(
@"
root
{
       _set{ path=/NONE to=5+5 }
}
");
      var result = new Azos.Conf.LaconicConfiguration();

      new ScriptRunner().Execute(src, result);
    }

    [Run]
    [Aver.Throws(typeof(ConfigException), Message = "is not after IF")]
    public void Error_ELSEWithoutIF()
    {
      var src = Azos.Conf.LaconicConfiguration.CreateFromString(
@"
root
{
       _else{  }
}
");
      var result = new Azos.Conf.LaconicConfiguration();

      new ScriptRunner().Execute(src, result);
    }

    [Run]
    [Aver.Throws(typeof(ConfigException), Message = "is not after IF")]
    public void Error_ELSEWithoutIF2()
    {
      var src = Azos.Conf.LaconicConfiguration.CreateFromString(
@"
root
{
       _if=true{}
       in-the-middle{}
       _else{  }
}
");
      var result = new Azos.Conf.LaconicConfiguration();

      new ScriptRunner().Execute(src, result);
    }


    [Run]
    [Aver.Throws(typeof(ConfigException), Message = "exceeded allowed timeout")]
    public void Error_Timeout()
    {
      var src = Azos.Conf.LaconicConfiguration.CreateFromString(
@"
root
{
       _loop=true {}
}
");
      var result = new Azos.Conf.LaconicConfiguration();

      new ScriptRunner().Execute(src, result);
    }




    [Run]
    public void SectionNameWithVar()
    {
      var src = Azos.Conf.LaconicConfiguration.CreateFromString(
@"
root
{
       i=0
       _loop=$(/$i)<10
       {
           section_$(/$i) {}
           _set{path=/$i to=$(/$i)+1}
       }
}
");
      var result = new Azos.Conf.LaconicConfiguration();

      new ScriptRunner().Execute(src, result);

      var got = result.SaveToString();
      got.See();

      Aver.AreEqual("section_0", result.Root[0].Name);
      Aver.AreEqual("section_9", result.Root[9].Name);
      Aver.IsFalse(result.Root[10].Exists);

    }


    const string rschema = @"
schema{
    PK_COLUMN=counter
    table=patient
    {
      column=$(/$PK_COLUMN) {type=TCounter required=true}
      column=resident_id {type=TMeaningfulID required=true}
      column=name {type=THumanName}
      _call=/AUDIT_COLUMNS {}
    }

    table=charge
    {
      column=$(/$PK_COLUMN) {type=TCounter required=true}
      column=transaction_date {type=TTimeStamp required=true}
      column=description {type=TDescription}
      column=amount {type=TMonetaryAmount}
      _call=/AUDIT_COLUMNS {}
    }


    AUDIT_COLUMNS
    {
        script-only=true
        column=change_user_id {type=TMeaningfulID required=true}
        column=change_date {type=TTimeStamp required=true}
    }

}";

    const string rschemaExpected =
    @"schema
{
  PK_COLUMN=counter
  table=patient
  {
    column=""$(/$PK_COLUMN)""
    {
      type=TCounter
      required=true
    }
    column=resident_id
    {
      type=TMeaningfulID
      required=true
    }
    column=name
    {
      type=THumanName
    }
    column=change_user_id
    {
      type=TMeaningfulID
      required=true
    }
    column=change_date
    {
      type=TTimeStamp
      required=true
    }
  }
  table=charge
  {
    column=""$(/$PK_COLUMN)""
    {
      type=TCounter
      required=true
    }
    column=transaction_date
    {
      type=TTimeStamp
      required=true
    }
    column=description
    {
      type=TDescription
    }
    column=amount
    {
      type=TMonetaryAmount
    }
    column=change_user_id
    {
      type=TMeaningfulID
      required=true
    }
    column=change_date
    {
      type=TTimeStamp
      required=true
    }
  }
}";


    [Run]
    public void RSchema()
    {
      var src = Azos.Conf.LaconicConfiguration.CreateFromString(rschema);
      var result = new Azos.Conf.LaconicConfiguration();

      new ScriptRunner().Execute(src, result);

      var got = result.SaveToString();
      got.See();
      Aver.AreEqual(rschemaExpected.ToWindowsLines(), got.ToWindowsLines());
    }


    [Run]
    public void LoopWithRealArithmetic()
    {
      var src = Azos.Conf.LaconicConfiguration.CreateFromString(
@"
root
{
       i=0
       _loop='$(/$i)<=2'
       {
           section_$(/$i) {}
           _set{path=/$i to=$(/$i)+0.5}
       }
}
");
      var result = new Azos.Conf.LaconicConfiguration();

      new ScriptRunner().Execute(src, result);

      var got = result.SaveToString();
      got.See();

      Aver.AreEqual("section_0", result.Root[0].Name);
      Aver.AreEqual("section_0.5", result.Root[1].Name);
      Aver.AreEqual("section_1", result.Root[2].Name);
      Aver.AreEqual("section_1.5", result.Root[3].Name);
      Aver.AreEqual("section_2", result.Root[4].Name);
      Aver.IsFalse(result.Root[5].Exists);
    }
  }
}
