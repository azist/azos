/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Linq;

using Azos.Conf;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration
{

    [Runnable]
    public class VarEvalTests
    {
        static string xml =
@"<root>

     <varEscaped>$(###)This is not var: $(../AAA)</varEscaped>
     <varIncomplete1>$()</varIncomplete1>
     <varIncomplete2>$(</varIncomplete2>

   <vars>

     <var1>val1</var1>
     <var2>$(../var1)</var2>

     <path1 value='c:\logs\' />
     <path2 value='\critical' />

     <many>
        <a value='1' age='18'>1</a>
        <a value='2' age='25'>2</a>
     </many>

     <var3>$(../var4)</var3>
     <var4>$(../var3)</var4>

     <var5>$(../var6)</var5>
     <var6>$(../var7)</var6>
     <var7>$(../var1)$(../var3)$(../var2)</var7>
   </vars>


   <MyClass>
    <data pvt-name='private'
          prot-name='protected'
          pub-name='public'
          age='99'>

          <extra
            enum='B'
            when='05/12/1982'
            cycle='$(/vars/var5)'
            >

            <fuzzy>true</fuzzy>
            <jazzy></jazzy>

          </extra>
    </data>
  </MyClass>

  <this name='$(/vars/var1)' text='This happened on $(../MyClass/data/extra/$when) date' />

  <that name='$(  /vars/var1  )' text='This happened on $( ../MyClass/data          /extra/$when  ) date' />

  <logger location='$(/vars/path1/$value)$(@/vars/path2/$value)'/>

  <optional>$(/non-existent)</optional>
  <required>$(!/non-existent)</required>

  <env1>$(~A)</env1>
  <env2>$(~A)+$(~B)</env2>
  <env3>$(~A)$(@~B)</env3>

  <envAppTopic>$(~App.CoreConsts.APPLICATION_TOPIC)</envAppTopic>
  <envDataTopic>$(~App.CoreConsts.DATA_TOPIC)</envDataTopic>
  <instanceID>$(~App.Instance)</instanceID>
  <counterA>$(~App.Counter.A)</counterA>
  <counterB>$(~App.Counter.B)</counterB>

  <coalesce>
    <a atr1='127'>1</a>
    <b>2</b>
    <d>$(/coalesce/a|../b)</d>
    <e>3</e>
    <f>$(../c|../d)777</f>
    <g>$(../c | ../d)8</g>
    <h>$(../c |../c|z|w| /coalesce/f)</h>
    <i>7$($c|../h)8</i>
    <j>$(../$c|../a/$atr1)</j>
    <k>100$(../$c ; ../a/$atr1)</k>
    <hang>200$(   ../a/$atr1  )</hang>
    <hang2>    300$(                ../$c      ;   ../a/$atr1      )    </hang2>

  </coalesce>

 </root>
";


        [Run]
        public void EscapedVar()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("This is not var: $(../AAA)", conf.Root.Navigate("/varEscaped").Value );
        }

        [Run]
        public void IncompleteVars()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("", conf.Root.Navigate("/varIncomplete1").Value );
          Aver.AreEqual("$(", conf.Root.Navigate("/varIncomplete2").Value );
        }


        [Aver.Throws(typeof(ConfigException), Message="not a section node")]
        [Run]
        public void BadPathWithAttrAttr()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          Aver.IsFalse( conf.Root.Navigate("/vars/path1/$value/$kaka").Exists );
        }


        [Run]
        public void PathWithPipes()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("\\critical", conf.Root.Navigate("/vars/paZZ1/$value|/vars/path2/$value").Value);
        }

        [Run]
        public void PathWithSectionIndexer()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("\\critical", conf.Root.Navigate("/vars/[3]/$value").Value);
        }

        [Run]
        public void PathWithAttributeIndexer()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("\\critical", conf.Root.Navigate("/vars/path2/$[0]").Value);
        }


        [Run]
        public void PathWithValueIndexer()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual(1, conf.Root.Navigate("/vars/many/a[1]").ValueAsInt());
          Aver.AreEqual(2, conf.Root.Navigate("/vars/many/a[2]").ValueAsInt());
        }

        [Run]
        public void PathWithAttributeValueIndexer()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual(1, conf.Root.Navigate("/vars/many/a[age=18]").ValueAsInt());
          Aver.AreEqual(2, conf.Root.Navigate("/vars/many/a[age=25]").ValueAsInt());
        }



        [Aver.Throws(typeof(ConfigException), Message="syntax")]
        [Run]
        public void PathWithBadIndexerSyntax1()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          conf.Root.Navigate("/]/$value");
        }

        [Aver.Throws(typeof(ConfigException), Message="syntax")]
        [Run]
        public void PathWithBadIndexerSyntax2()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          conf.Root.Navigate("/aaa]/$value");
        }



        [Aver.Throws(typeof(ConfigException), Message="syntax")]
        [Run]
        public void PathWithBadIndexerSyntax3()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          conf.Root.Navigate("/[/$value");
        }




        [Run]
        public void TestNavigationinVarNames()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);


          Aver.AreEqual("val1", conf.Root["vars"]["var1"].Value);
          Aver.AreEqual("val1", conf.Root["vars"]["var1"].VerbatimValue);

          Aver.AreEqual("val1", conf.Root["vars"]["var2"].Value);
          Aver.AreEqual("$(../var1)", conf.Root["vars"]["var2"].VerbatimValue);


          Aver.AreEqual("val1", conf.Root["this"].AttrByName("name").Value);
          Aver.AreEqual("$(/vars/var1)", conf.Root["this"].AttrByName("name").VerbatimValue);
          Aver.AreEqual("$(/vars/var1)", conf.Root["this"].AttrByName("name").ValueAsString(verbatim: true));

          Aver.AreEqual("This happened on 05/12/1982 date", conf.Root["this"].AttrByName("text").Value);

          Aver.AreEqual(@"c:\logs\critical", conf.Root["logger"].AttrByName("location").Value);

        }

        [Run]
        public void TestNavigationinVarNames_WithSpaces()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("val1", conf.Root["that"].AttrByName("name").Value);
          Aver.AreEqual("$(  /vars/var1  )", conf.Root["that"].AttrByName("name").VerbatimValue);
          Aver.AreEqual("$(  /vars/var1  )", conf.Root["that"].AttrByName("name").ValueAsString(verbatim: true));

          Aver.AreEqual("This happened on 05/12/1982 date", conf.Root["that"].AttrByName("text").Value);
        }

        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void Recursive1()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("$(../var4)", conf.Root["vars"]["var3"].VerbatimValue);//no exception
          var v = conf.Root["vars"]["var3"].Value;
        }

        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void Recursive2()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("$(../var3)", conf.Root["vars"]["var4"].VerbatimValue);//no exception
          var v = conf.Root["vars"]["var4"].Value;
        }

        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void Recursive3Transitive()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          var attr = conf.Root["MyClass"]["data"]["extra"].AttrByName("cycle");

          var v1 = attr.VerbatimValue;//no exception
          var v2 = attr.Value;//exception
        }


        [Run]
        public void Recursive4StackCleanup()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          var attr = conf.Root["MyClass"]["data"]["extra"].AttrByName("cycle");

          try
          {
           var v2 = attr.Value;//exception
          }
          catch(Exception error)
          {
           Console.WriteLine("Expected and got: "+error.Message);
          }

          //after exception, stack should cleanup and work again as expected
          Aver.AreEqual("val1", conf.Root["vars"]["var1"].Value);
        }


        [Run]
        public void Optional()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual(true, string.IsNullOrEmpty(conf.Root["optional"].Value));
        }

        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void Required()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          var v = conf.Root["required"].Value;
        }



        [Run]
        public void EnvVars1()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);
          conf.EnvironmentVarResolver = new MyVars();

           Aver.AreEqual("1", conf.Root["env1"].Value);
           Aver.AreEqual("1+2", conf.Root["env2"].Value);
           Aver.AreEqual(@"1\2", conf.Root["env3"].Value);

        }


        [Run]
        public void EvalFromString_1()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

           Aver.AreEqual("Hello val1", "Hello $(vars/var1)".EvaluateVarsInConfigScope(conf));
           Aver.AreEqual("Hello val1", "Hello $(vars/var1)".EvaluateVarsInXMLConfigScope(xml));
           Aver.AreEqual("Hello 123 suslik!", "Hello $(/$v) suslik!".EvaluateVarsInXMLConfigScope("<a v='123'> </a>"));
        }

        [Run]
        public void EvalFromString_2_manysame()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

           Aver.AreEqual("Hello val1val1val1", "Hello $(vars/var1)$(vars/var1)$(vars/var1)".EvaluateVarsInConfigScope(conf));
        }



        [Run]
        public void EvalFromStringWithEnvVarResolver()
        {
           Aver.AreEqual("Time is 01/18/1901 2:03PM", "Time is $(~C)".EvaluateVars( new MyVars()));
        }


        [Run]
        public void EvalFromStringWithEnvVarInline()
        {
           Aver.AreEqual("Hello, your age is 123", "$(~GreEtInG), your age is $(~AGE)".EvaluateVars( new Vars(new VarsDictionary {
                            {"Greeting", "Hello"},
                            {"Age", "123"}
           })));
        }


        [Run]
        public void EvalFromStringWithEnvVarAndMacro()
        {
           Aver.AreEqual("Time is 01/1901", "Time is $(~C::as-dateTime fmt=\"{0:MM/yyyy}\")".EvaluateVars( new MyVars()));
        }

        [Run]
        public void EvalFromStringWithEnvVarAndMacro2()
        {
           Aver.AreEqual("Time is Month=01 Year=1901", "Time is $(~C::as-dateTime fmt=\"Month={0:MM} Year={0:yyyy}\")".EvaluateVars( new MyVars()));
        }


        [Run]
        public void EvalFromStringMacroDefault()
        {
           Aver.AreEqual("Value is 12 OK?", "Value is $(/dont-exist::as-int dflt=\"12\") OK?".EvaluateVars());
        }

        [Run]
        public void EvalFromStringMacroDefault2()
        {
           Aver.AreEqual("James, the value is 12 OK?",
                           "$(/$name::as-string dflt=\"James\"), the value is $(/dont-exist::as-int dflt=\"12\") OK?".EvaluateVars());
        }

        [Run]
        public void EvalFromStringMacroDefault3()
        {
           Aver.AreEqual("Mark Spenser, the value is 12 OK?",
                           "$(~name::as-string dflt=\"James\"), the value is $(/dont-exist::as-int dflt=\"12\") OK?".EvaluateVars(
                            new Vars( new VarsDictionary { {"name", "Mark Spenser"}  })
                           ));
        }

        [Run]
        public void EvalTestNowString()
        {
            Aver.AreEqual("20131012-06", "$(::now fmt=yyyyMMdd-HH value=20131012-06)".EvaluateVars());
        }

        [Run]
        public void NodePaths()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);


          Aver.AreEqual("/vars/path2", conf.Root["vars"]["path2"].RootPath);
          Aver.AreEqual("/vars/path2/$value", conf.Root["vars"]["path2"].AttrByIndex(0).RootPath);
          Aver.AreEqual("/vars/many/[0]", conf.Root["vars"]["many"][0].RootPath);
          Aver.AreEqual("/vars/many/[1]", conf.Root["vars"]["many"][1].RootPath);
          Aver.AreEqual("/vars/many/[1]/$value", conf.Root["vars"]["many"][1].AttrByIndex(0).RootPath);
        }


        [Run]
        public void NavigateBackToNodePaths()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          Aver.AreEqual("/vars/path2",           conf.Root.Navigate( conf.Root["vars"]["path2"].RootPath                  ).RootPath);
          Aver.AreEqual("/vars/path2/$value",    conf.Root.Navigate( conf.Root["vars"]["path2"].AttrByIndex(0).RootPath   ).RootPath);
          Aver.AreEqual("/vars/many/[0]",        conf.Root.Navigate( conf.Root["vars"]["many"][0].RootPath                ).RootPath);
          Aver.AreEqual("/vars/many/[1]",        conf.Root.Navigate( conf.Root["vars"]["many"][1].RootPath                ).RootPath);
          Aver.AreEqual("/vars/many/[1]/$value", conf.Root.Navigate( conf.Root["vars"]["many"][1].AttrByIndex(0).RootPath ).RootPath);
        }


        [Run]
        public void AppVars()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);
          Aver.AreEqual( CoreConsts.APPLICATION_TOPIC, conf.Root["envAppTopic"].Value );
          Aver.AreEqual( CoreConsts.DATA_TOPIC, conf.Root["envDataTopic"].Value );
          Aver.AreEqual(Apps.ExecutionContext.Application.InstanceId, conf.Root["instanceID"].ValueAsGUID(Guid.Empty));

          Aver.AreEqual(CoreConsts.APPLICATION_TOPIC, conf.Root["envAppTopic"].Value);

          Aver.AreEqual(1, conf.Root["counterA"].ValueAsInt());
          Aver.AreEqual(2, conf.Root["counterA"].ValueAsInt());
          Aver.AreEqual(3, conf.Root["counterA"].ValueAsInt());
          Aver.AreEqual(1, conf.Root["counterB"].ValueAsInt());
        }

    [Run]
    public void CoalesceMultipleVarReferences()
    {
      var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml).Root;

      var coalesce = conf["coalesce"];
      Aver.IsTrue(coalesce.Exists);

      Aver.AreEqual(1, coalesce["a"].ValueAsInt());
      Aver.AreEqual(2, coalesce["b"].ValueAsInt());
      Aver.AreEqual(0, coalesce["c"].ValueAsInt());
      Aver.AreEqual(1, coalesce["d"].ValueAsInt());
      Aver.AreEqual(3, coalesce["e"].ValueAsInt());
      Aver.AreEqual(1777, coalesce["f"].ValueAsInt());
      Aver.AreEqual(18, coalesce["g"].ValueAsInt());
      Aver.AreEqual(1777, coalesce["h"].ValueAsInt());
      Aver.AreEqual(717778, coalesce["i"].ValueAsInt());
      Aver.AreEqual(127, coalesce["j"].ValueAsInt());
      Aver.AreEqual(100127, coalesce["k"].ValueAsInt());
      Aver.AreEqual(200127, coalesce["hang"].ValueAsInt());
      Aver.AreEqual(300127, coalesce["hang2"].ValueAsInt());
    }

    [Run]
    public void MaxIterationsLimit()
    {
      var conf = "app{ a=$(~A) b=$($a) }".AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);

      var external = new Vars();
      external["A"] = "$(~A) something";   //this creates an infinite loop as var resolves with another reference to itself
      conf.Configuration.EnvironmentVarResolver = external;

      try
      {
        var crash = conf.AttrByName("b").Value;
        Aver.Fail("We should have received an exception about config var limit");
      }
      catch(ConfigException error)
      {
        Console.WriteLine("Expected and got ConfigException: {0}".Args(error.Message));
      }
    }

  }


    class MyVars : IEnvironmentVariableResolver
    {

      public bool ResolveEnvironmentVariable(string name, out string value)
      {
        value = null;
        if (name == "A") value = "1";
        if (name == "B") value = "2";
        if (name ==  "C") value = "01/18/1901 2:03PM";
        return true;
      }
    }




}



