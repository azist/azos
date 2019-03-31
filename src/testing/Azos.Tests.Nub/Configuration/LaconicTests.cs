/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using Azos.Scripting;

using Azos.Conf;
using Azos.Data;
using Azos.CodeAnalysis.Laconfig;

namespace Azos.Tests.Nub.Configuration
{
    [Runnable]
    public class LaconicTests
    {

    static string src1 =
@"
root
{
   vars
   {
     var1=val1{}
     var2=$(../var1){}

     path1{ value=c:\logs\ }
     path2{ value=\critical }

     many
     {
        a{ value=1}
        a{ value=2}
     }

     var3=$(../var4){}
     var4=$(../var3){}

     var5=$(../var6){}
     var6=$(../var7){}
     var7=$(../var1)$(../var3)$(../var2){}
   }


   MyClass
   {
    data
    {  pvt-name='private'
       prot-name='protected'
       pub-name='public'
       age='99'

          extra
          {
            enum='B'
            when='05/12/1982'
            cycle='$(/vars/var5)'


            fuzzy=true{}
            jazzy=''{}

          }
    }//data
   }//MyClass

  this { name=$(/vars/var1) text='This happened on $(../MyClass/data/extra/$when) date'}

  logger{ location=$(/vars/path1/$value)$(@/vars/path2/$value)}

  optional=$(/non-existent){}
  required=$(!/non-existent){}

  env1=$(~A){}
  env2=$(~A)+$(~B){}
  env3=$(~A)$(@~B){}

  bytes='3d,12,ff'

 }//root
";



        [Run]
        public void JustRoot()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString("root{a=$(100+200)}");

          Aver.AreEqual("root", conf.Root.Name);
        }

        [Run]
        public void RootAndSub()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString("root{sub{}}");

          Aver.AreEqual("root", conf.Root.Name);
          Aver.AreEqual("sub", conf.Root["sub"].Name);
        }

        [Run]
        public void RootAttrAsByteArray()
        {
          var root = src1.AsLaconicConfig();

          var bytes = root.AttrByName("bytes").ValueAsByteArray();
          Aver.AreEqual(3, bytes.Length);
          Aver.AreEqual(0x3d, bytes[0]);
          Aver.AreEqual(0x12, bytes[1]);
          Aver.AreEqual(0xff, bytes[2]);
        }

        [Run]
        public void RootWithAttrAndSub()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString("root{ atr=val sub{}}");

          Aver.AreEqual("root", conf.Root.Name);
          Aver.IsTrue(conf.Root.AttrByName("atr").Exists);
          Aver.AreEqual("val", conf.Root.AttrByName("atr").Value);
        }

        [Run]
        public void RootWithAttrWithSpace()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString("root{ atr = val }");

          Aver.AreEqual("root", conf.Root.Name);
          Aver.IsTrue(conf.Root.AttrByName("atr").Exists);
          Aver.AreEqual("val", conf.Root.AttrByName("atr").Value);
        }

        [Run]
        public void RootWithAttrWithSpace2()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString("root{ atr = val atr2   =                                 'string   3'}");

          Aver.AreEqual("root", conf.Root.Name);
          Aver.IsTrue(conf.Root.AttrByName("atr").Exists);
          Aver.AreEqual("val", conf.Root.AttrByName("atr").Value);
          Aver.IsTrue(conf.Root.AttrByName("atr2").Exists);
          Aver.AreEqual("string   3", conf.Root.AttrByName("atr2").Value);
        }



        [Run]
        public void RootWithManyAttrs()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString("root{ atr1=val1 atr2=val2 'atr 3'='value{3}'}");

          Aver.AreEqual("root", conf.Root.Name);
          Aver.AreEqual("val1", conf.Root.AttrByName("atr1").Value);
          Aver.AreEqual("val2", conf.Root.AttrByName("atr2").Value);
          Aver.AreEqual("value{3}", conf.Root.AttrByName("atr 3").Value);
        }

        [Run]
        public void RootWithManyAttrsAndSectionValues()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString("root=yes{ atr1=val1 atr2=val2 'atr 3'='value{3}' log=12{}}");

          Aver.AreEqual("root", conf.Root.Name);
          Aver.AreEqual("yes", conf.Root.Value);
          Aver.AreEqual("val1", conf.Root.AttrByName("atr1").Value);
          Aver.AreEqual("val2", conf.Root.AttrByName("atr2").Value);
          Aver.AreEqual("value{3}", conf.Root.AttrByName("atr 3").Value);

          Aver.AreEqual("12", conf.Root["log"].Value);
        }

        [Run]
        public void TestNavigationinVarNames()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString(src1);


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
        public void TestNavigationinVarNames_Parallel()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString(src1);

          System.Threading.Tasks.Parallel.For(0, 100000,
            (_)=>
            {
                Aver.AreEqual("val1", conf.Root["vars"]["var1"].Value);
                Aver.AreEqual("val1", conf.Root["vars"]["var1"].VerbatimValue);

                Aver.AreEqual("val1", conf.Root["vars"]["var2"].Value);
                Aver.AreEqual("$(../var1)", conf.Root["vars"]["var2"].VerbatimValue);


                Aver.AreEqual("val1", conf.Root["this"].AttrByName("name").Value);
                Aver.AreEqual("$(/vars/var1)", conf.Root["this"].AttrByName("name").VerbatimValue);
                Aver.AreEqual("$(/vars/var1)", conf.Root["this"].AttrByName("name").ValueAsString(verbatim: true));

                Aver.AreEqual("This happened on 05/12/1982 date", conf.Root["this"].AttrByName("text").Value);

                Aver.AreEqual(@"c:\logs\critical", conf.Root["logger"].AttrByName("location").Value);
            });
        }



        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void Recursive1()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString(src1);

          Aver.AreEqual("$(../var4)", conf.Root["vars"]["var3"].VerbatimValue);//no exception
          var v = conf.Root["vars"]["var3"].Value;
        }

        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void Recursive2()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString(src1);

          Aver.AreEqual("$(../var3)", conf.Root["vars"]["var4"].VerbatimValue);//no exception
          var v = conf.Root["vars"]["var4"].Value;
        }

        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void Recursive3Transitive()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString(src1);

          var attr = conf.Root["MyClass"]["data"]["extra"].AttrByName("cycle");

          var v1 = attr.VerbatimValue;//no exception
          var v2 = attr.Value;//exception
        }


        [Run]
        public void Recursive4StackCleanup()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString(src1);

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
        public void Recursive4StackCleanup_Parallel()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString(src1);

          System.Threading.Tasks.Parallel.For(0, 25_000,
            (_)=>
            {
                var attr = conf.Root["MyClass"]["data"]["extra"].AttrByName("cycle");

                try
                {
                 var v2 = attr.Value;//exception
                }
                catch(Exception error)
                {
                 Aver.IsTrue( error.Message.Contains("recursive vars"));
                }

                //after exception, stack should cleanup and work again as expected
                Aver.AreEqual("val1", conf.Root["vars"]["var1"].Value);
          });
        }




        [Run]
        public void Optional()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString(src1);

          Aver.AreEqual(true, string.IsNullOrEmpty(conf.Root["optional"].Value));
        }

        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void Required()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString(src1);

          var v = conf.Root["required"].Value;
        }



        [Run]
        public void EnvVars1()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString(src1);
          conf.EnvironmentVarResolver = new MyVars();

           Aver.AreEqual("1", conf.Root["env1"].Value);
           Aver.AreEqual("1+2", conf.Root["env2"].Value);
           Aver.AreEqual(@"1\2", conf.Root["env3"].Value);

        }


        [Run]
        public void NodePaths()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString(src1);



          Aver.AreEqual("/vars/path2", conf.Root["vars"]["path2"].RootPath);
          Aver.AreEqual("/vars/path2/$value", conf.Root["vars"]["path2"].AttrByIndex(0).RootPath);
          Aver.AreEqual("/vars/many/[0]", conf.Root["vars"]["many"][0].RootPath);
          Aver.AreEqual("/vars/many/[1]", conf.Root["vars"]["many"][1].RootPath);
          Aver.AreEqual("/vars/many/[1]/$value", conf.Root["vars"]["many"][1].AttrByIndex(0).RootPath);
        }


        [Run]
        public void NodePaths_Parallel()
        {
          var conf = Azos.Conf.LaconicConfiguration.CreateFromString(src1);

           System.Threading.Tasks.Parallel.For(0, 100000,
            (_)=>
            {
                Aver.AreEqual("/vars/path2", conf.Root["vars"]["path2"].RootPath);
                Aver.AreEqual("/vars/path2/$value", conf.Root["vars"]["path2"].AttrByIndex(0).RootPath);
                Aver.AreEqual("/vars/many/[0]", conf.Root["vars"]["many"][0].RootPath);
                Aver.AreEqual("/vars/many/[1]", conf.Root["vars"]["many"][1].RootPath);
                Aver.AreEqual("/vars/many/[1]/$value", conf.Root["vars"]["many"][1].AttrByIndex(0).RootPath);
            });
        }


        [Run]
        public void SaveToString()
        {
          var conf = new LaconicConfiguration();
          conf.Create("very-root");
          conf.Root.AddChildNode("childSection1").AddAttributeNode("name", "Alex");
          conf.Root.AddChildNode("child2").AddAttributeNode("name", "Boris");
          conf.Root["child2"].Value = "Muxa";

          var child3 = conf.Root.AddChildNode("child3");
          child3.AddAttributeNode("atr with space", 1);
          child3.AddAttributeNode("atr2", "val with space");
          child3.AddAttributeNode("atr{3}", null);
          child3.AddAttributeNode("atr=4", null);

          child3.AddAttributeNode("atr5", "this goes on \n\r new\\next line");

          child3.AddChildNode("child3.1");
          child3.AddChildNode("child3.2").AddChildNode("child3.2.1");
          child3.AddChildNode("child3.3");

          var txt = conf.SaveToString(LaconfigWritingOptions.PrettyPrint);
          Console.WriteLine(txt);

          Aver.AreEqual(
@"very-root
{
  childSection1
  {
    name=Alex
  }
  child2=Muxa
  {
    name=Boris
  }
  child3
  {
    ""atr with space""=1
    atr2=""val with space""
    ""atr{3}""=''
    ""atr=4""=''
    atr5=""this goes on \n\r new\\next line""
    child3.1
    {
    }
    child3.2
    {
      child3.2.1
      {
      }
    }
    child3.3
    {
    }
  }
}".TrimAll(), txt.TrimAll());

        txt = conf.SaveToString(LaconfigWritingOptions.Compact);

        Console.WriteLine(txt);

        Aver.AreEqual(
 @"very-root{childSection1{name=Alex}child2=Muxa{name=Boris}child3{""atr with space""=1 atr2=""val with space"" ""atr{3}""='' ""atr=4""='' atr5=""this goes on \n\r new\\next line"" child3.1{}child3.2{child3.2.1{}}child3.3{}}}",
   txt);
        }


        [Run]
        public void SaveToPrettyStringThenReadBack()
        {
          var conf = new LaconicConfiguration();
          conf.Create("very-root");
          conf.Root.AddChildNode("childSection1").AddAttributeNode("name", "Alex");
          conf.Root.AddChildNode("child2").AddAttributeNode("name", "Boris");
          conf.Root["child2"].Value = "Muxa";

          var child3 = conf.Root.AddChildNode("child3");
          child3.AddAttributeNode("atr with space", 1);
          child3.AddAttributeNode("atr2", "val with space");
          child3.AddAttributeNode("atr{3}", null);
          child3.AddAttributeNode("atr=4", null);

          child3.AddAttributeNode("atr5", "this goes on \n\r new\\next line");

          child3.AddChildNode("child3.1");
          child3.AddChildNode("child3.2").AddChildNode("child3.2.1");
          child3.AddChildNode("child3.3");

          var txt =  conf.SaveToString(LaconfigWritingOptions.PrettyPrint);

          var conf2 = LaconicConfiguration.CreateFromString(txt);

          Aver.IsTrue(conf2.Root["childSection1"].Exists);
          Aver.IsTrue(conf2.Root["child3"].AttrByName("atr with space").Exists);
          Aver.IsTrue(conf2.Root.Navigate("childSection1/$name").Exists);
          Aver.IsTrue(conf2.Root.Navigate("child2/$name").Exists);
          Aver.IsTrue(conf2.Root.Navigate("child3/$atr{3}").Exists);
          Aver.IsTrue(conf2.Root.Navigate("child3/child3.2/child3.2.1").Exists);

          Aver.AreEqual("Muxa", conf2.Root.Navigate("child2").Value);
          Aver.AreEqual("1", conf2.Root.Navigate("child3/$atr with space").Value);
          Aver.AreEqual("val with space", conf2.Root.Navigate("child3/$atr2").Value);
          Aver.IsTrue( conf2.Root.Navigate("child3/$atr=4").Value.IsNullOrWhiteSpace() );
        }

        [Run]
        public void SaveToCompactStringThenReadBack()
        {
          var conf = new LaconicConfiguration();
          conf.Create("very-root");
          conf.Root.AddChildNode("childSection1").AddAttributeNode("name", "Alex");
          conf.Root.AddChildNode("child2").AddAttributeNode("name", "Boris");
          conf.Root["child2"].Value = "Muxa";

          var child3 = conf.Root.AddChildNode("child3");
          child3.AddAttributeNode("atr with space", 1);
          child3.AddAttributeNode("atr2", "val with space");
          child3.AddAttributeNode("atr{3}", null);
          child3.AddAttributeNode("atr=4", null);

          child3.AddAttributeNode("atr5", "this goes on \n\r new\\next line");

          child3.AddChildNode("child3.1");
          child3.AddChildNode("child3.2").AddChildNode("child3.2.1");
          child3.AddChildNode("child3.3");

          var txt =  conf.SaveToString(LaconfigWritingOptions.Compact);
Console.WriteLine(txt);
          var conf2 = LaconicConfiguration.CreateFromString(txt);

          Aver.IsTrue(conf2.Root["childSection1"].Exists);
          Aver.IsTrue(conf2.Root["child3"].AttrByName("atr with space").Exists);
          Aver.IsTrue(conf2.Root.Navigate("childSection1/$name").Exists);
          Aver.IsTrue(conf2.Root.Navigate("child2/$name").Exists);
          Aver.IsTrue(conf2.Root.Navigate("child3/$atr{3}").Exists);
          Aver.IsTrue(conf2.Root.Navigate("child3/child3.2/child3.2.1").Exists);

          Aver.AreEqual("Muxa", conf2.Root.Navigate("child2").Value);
          Aver.AreEqual("1", conf2.Root.Navigate("child3/$atr with space").Value);
          Aver.AreEqual("val with space", conf2.Root.Navigate("child3/$atr2").Value);
          Aver.IsTrue( conf2.Root.Navigate("child3/$atr=4").Value.IsNullOrWhiteSpace() );
        }



        [Run]
        public void AddChildSectionFromClone()
        {
          var conf1 = Azos.Conf.LaconicConfiguration.CreateFromString("root{}");
          var conf2 = Azos.Conf.LaconicConfiguration.CreateFromString("root{ sect1{ a=1 b=2 subsect1{c=3 d=4}}}");

          conf1.Root.AddChildNode(conf2.Root["sect1"]);

          Console.WriteLine(conf1.ContentView);

          Aver.AreEqual(1, conf1.Root.Children.Count());
          Aver.AreEqual("sect1", conf1.Root["sect1"].Name);
          Aver.AreEqual(2, conf1.Root["sect1"].Attributes.Count());
          Aver.AreEqual(1, conf1.Root["sect1"].AttrByName("a").ValueAsInt());
          Aver.AreEqual(2, conf1.Root["sect1"].AttrByName("b").ValueAsInt());
          Aver.AreEqual(3, conf1.Root["sect1"]["subsect1"].AttrByName("c").ValueAsInt());
          Aver.AreEqual(4, conf1.Root["sect1"]["subsect1"].AttrByName("d").ValueAsInt());
        }


        [Run]
        public void Options_DontWriteRootSectionDeclaration()
        {
          var conf = "app=90{a=1 b=2 c{d=5}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          var opt = new LaconfigWritingOptions{ DontWriteRootSectionDeclaration = true };

          var saved = conf.Configuration.ToLaconicString(opt);
          Console.WriteLine(saved);

          Aver.AreEqual("a=1 b=2 c{d=5}", saved);

          opt = new LaconfigWritingOptions{ /* DontWriteRootSectionDeclaration = false - default */ };

          saved = conf.Configuration.ToLaconicString(opt).Trim();
          Console.WriteLine(saved);

          Aver.AreEqual("app=90{a=1 b=2 c{d=5}}", saved);
        }


    }

}
