/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Conf;
using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Unit.Config
{
    [Runnable(TRUN.BASE)]
    public class FSIncludesTests
    {

static string xml1 = @"
 <root>

    <section-a>
        <sub1>Sub Value 1</sub1>
        <sub2>Sub Value 2</sub2>
        <sub3>Sub Value 3</sub3>
        <destination name='A' type='CSVFile'> </destination>
        <destination name='B' type='SMTPMail'> </destination>
    </section-a>

    <section-b _override='attributes' age='32'>

    </section-b>

    <section-c _override='fail'>
       This can not be overridden and exception will be thrown
    </section-c>

    <section-d _override='stop'>This can not be overridden and no exception will be thrown</section-d>

    <section-e _override='replace' some-attr='123'>
       <a> </a>
       <b> </b>
    </section-e>

    <section-f _override='sections' some-attr='423'>
       <a> </a>
       <b> </b>
    </section-f>

 </root>
";


static string xml2 = @"
 <root
   meduza='Greece'
 >

    <a />
    <b />
    <c yes='true'/>



 </root>
";

        [Run]
        public void Include1()
        {
          var conf1 = Azos.Conf.XMLConfiguration.CreateFromXML(xml1);
          var conf2 = Azos.Conf.XMLConfiguration.CreateFromXML(xml2);

          conf1.Include(conf1.Root["section-a"], conf2.Root);


          Aver.IsFalse(conf1.Root["section-a"].Exists);
          Aver.IsTrue(conf1.Root.AttrByName("meduza").Exists);
          Aver.IsTrue(conf1.Root["a"].Exists);
          Aver.IsTrue(conf1.Root["b"].Exists);
          Aver.IsTrue(conf1.Root["c"].Exists);



          Aver.AreEqual("Greece", conf1.Root.AttrByName("meduza").Value);
          Aver.AreEqual(true, conf1.Root["c"].AttrByName("yes").ValueAsBool());
        }

        [Run]
        public void Include2_SequencingAtStart()
        {
          var conf1 = Azos.Conf.XMLConfiguration.CreateFromXML(xml1);
          var conf2 = Azos.Conf.XMLConfiguration.CreateFromXML(xml2);

          conf1.Include(conf1.Root["section-a"], conf2.Root);

          var lst = conf1.Root.Children.ToList();

          Aver.AreEqual(8, lst.Count);

          Aver.AreEqual("a", lst[0].Name);
          Aver.AreEqual("b", lst[1].Name);
          Aver.AreEqual("c", lst[2].Name);
          Aver.AreEqual("section-b", lst[3].Name);
          Aver.AreEqual("section-c", lst[4].Name);
          Aver.AreEqual("section-d", lst[5].Name);
          Aver.AreEqual("section-e", lst[6].Name);
          Aver.AreEqual("section-f", lst[7].Name);


        }

        [Run]
        public void Include2_SequencingAtEnd()
        {
          var conf1 = Azos.Conf.XMLConfiguration.CreateFromXML(xml1);
          var conf2 = Azos.Conf.XMLConfiguration.CreateFromXML(xml2);

          conf1.Include(conf1.Root["section-f"], conf2.Root);

          var lst = conf1.Root.Children.ToList();

          Aver.AreEqual(8, lst.Count);

          Aver.AreEqual("section-a", lst[0].Name);
          Aver.AreEqual("section-b", lst[1].Name);
          Aver.AreEqual("section-c", lst[2].Name);
          Aver.AreEqual("section-d", lst[3].Name);
          Aver.AreEqual("section-e", lst[4].Name);
          Aver.AreEqual("a", lst[5].Name);
          Aver.AreEqual("b", lst[6].Name);
          Aver.AreEqual("c", lst[7].Name);


        }

        [Run]
        public void PRAGMA_1()
        {

#warning REFACTOR env variable!!!!
          var conf1 =
@"
nfx
{
  a=1
  _include
  {
    name=WithNewName
    file=$""[[0]]""
  }

  _include
  {
    //without name
    file=$""[[0]]""
  }

}
".Replace("[[0]]", System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("AZIST_HOME"), "AZOS", "out","Debug","UTEZT-1.laconf") );

            var conf = conf1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );

            Console.WriteLine(conf.ToLaconicString());

            conf.ProcessIncludePragmas(true);

            Console.WriteLine("============== AFTER PROCESS INCLUDE PRAGMAS ==================");
            Console.WriteLine(conf.ToLaconicString());

            Aver.AreEqual(189, conf.Navigate("/$a").ValueAsInt());
            Aver.AreEqual(2, conf.Navigate("/$file.b").ValueAsInt());
            Aver.AreEqual(3, conf.Navigate("/$file.c").ValueAsInt());

            Aver.AreEqual(189, conf.Navigate("/WithNewName/$a").ValueAsInt());
            Aver.AreEqual(2, conf.Navigate("/WithNewName/$file.b").ValueAsInt());
            Aver.AreEqual(3, conf.Navigate("/WithNewName/$file.c").ValueAsInt());

            Aver.AreEqual("another one", conf.Navigate("/WithNewName/sub-sect/$name").Value);

        }


                   public class TeztConfigNodeProvider : IConfigNodeProvider
                   {

                     public ConfigSectionNode ProvideConfigNode(object context = null)
                     {
                       return @"zhaba{ _override='all' age=129 people{  a=Alex{}  b=Boris{} }  }".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
                     }

                     public void Configure(IConfigSectionNode node)
                     {
                       Console.WriteLine("Configuring");
                     }
                   }



        [Run]
        public void Include_Provider()
        {
          var conf =@"
myapp
{
  _include
  {
    name=WithNewName
    provider{ type='Azos.Tests.Unit.Config.Includes+TeztConfigNodeProvider, Azos.Tests.Unit'}
  }

  city{ name='Cleveland'}

  _include
  {
    //without name
    provider{ type='Azos.Tests.Unit.Config.Includes+TeztConfigNodeProvider, Azos.Tests.Unit'}
  }
}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);


          conf.ProcessIncludePragmas(true);

          Console.WriteLine( conf.ToLaconicString(Azos.CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint) );

          Aver.AreEqual(3, conf.ChildCount);

          Aver.AreEqual(129, conf.AttrByName("age").ValueAsInt());
          Aver.AreEqual("all", conf.AttrByName("_override").Value);

          Aver.AreEqual("Alex", conf.Navigate("/WithNewName/people/a").Value);
          Aver.AreEqual("Boris", conf.Navigate("/WithNewName/people/b").Value);

          Aver.AreEqual("Cleveland", conf.Navigate("/city/$name").Value);

          Aver.AreEqual("Alex", conf.Navigate("/people/a").Value);
          Aver.AreEqual("Boris", conf.Navigate("/people/b").Value);


        }
  }
}
