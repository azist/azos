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

namespace Azos.Tests.Nub.Configuration
{
    [Runnable]
    public class OverridesAndMergesTests
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
 <root>

    <section-a newattr='new val'>
        <sub2>Sub Value 2 ammended</sub2>
        <sub3>Sub Value 3</sub3>
        <destination name='B' type='Clock'> </destination>
    </section-a>

    <section-b _override='attributes' age='89' pension='true'>

    </section-b>

    <section-d all='yes'>
       This will be ignored
    </section-d>

    <section-e _override='replace' some-attr='992'>
       <demo>Demo!</demo>
    </section-e>

    <section-f _override='sections' some-attr='324'>
       <_clear/>
       <bim good='yes'> </bim>
    </section-f>

 </root>
";

static string xml3 = @"
 <root>
    <section-c>
       Will cause failure if merged with XML1
    </section-c>
 </root>
";

static string xml4 = @"
 <root>
    <section-a>
      <_delete/>
    </section-a>
    <section-b _override='attributes' _clear='true'>

    </section-b>
 </root>
";



        [Run]
        public void BasicMerge()
        {
          var conf1 = Azos.Conf.XMLConfiguration.CreateFromXML(xml1);
          var conf2 = Azos.Conf.XMLConfiguration.CreateFromXML(xml2);
          var conf3 = Azos.Conf.XMLConfiguration.CreateFromXML(xml4);

          var conf = new Azos.Conf.MemoryConfiguration();
          conf.CreateFromMerge(conf1.Root, conf2.Root);

          var conf4 = new Azos.Conf.MemoryConfiguration();
          conf4.CreateFromMerge(conf.Root, conf3.Root);

          Aver.AreEqual("Sub Value 1", conf.Root["section-a"]["sub1"].Value);
          Aver.AreEqual("Sub Value 2 ammended", conf.Root["section-a"]["sub2"].Value);
          Aver.AreEqual("Sub Value 3", conf.Root["section-a"]["sub3"].Value);

          Aver.AreEqual("CSVFile", conf.Root["section-a"].Children.FirstOrDefault(n=>n.IsSameName("destination") && n.AttrByName("name").Value=="A").AttrByName("type").Value);
          Aver.AreEqual("Clock", conf.Root["section-a"].Children.FirstOrDefault(n=>n.IsSameName("destination") && n.AttrByName("name").Value=="B").AttrByName("type").Value);
          Aver.AreEqual("SMTPMail", conf1.Root["section-a"].Children.FirstOrDefault(n=>n.IsSameName("destination") && n.AttrByName("name").Value=="B").AttrByName("type").Value);

          Aver.IsTrue(conf4.Root["section-b"].AttrCount == 1);
          Aver.IsTrue(!conf4.Root["section-a"].Exists);
        }

        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void ExpectOverrideException()
        {
          var conf1 = Azos.Conf.XMLConfiguration.CreateFromXML(xml1);
          var conf2 = Azos.Conf.XMLConfiguration.CreateFromXML(xml3);

          var conf = new Azos.Conf.MemoryConfiguration();

          try
          {
            conf.CreateFromMerge(conf1.Root, conf2.Root);
          }
          catch(Exception error)
          {
            Console.WriteLine("Expected and got: "+error.Message);
            throw error;
          }

        }


        [Run]
        public void MergeStop()
        {
          var conf1 = Azos.Conf.XMLConfiguration.CreateFromXML(xml1);
          var conf2 = Azos.Conf.XMLConfiguration.CreateFromXML(xml2);

          var conf = new Azos.Conf.MemoryConfiguration();
          conf.CreateFromMerge(conf1.Root, conf2.Root);


          Aver.AreEqual("This can not be overridden and no exception will be thrown", conf.Root["section-d"].Value);
          Aver.AreEqual(1, conf.Root["section-d"].Attributes.Count());
          Aver.IsTrue(OverrideSpec.Stop == NodeOverrideRules.Default.StringToOverrideSpec(conf.Root["section-d"].AttrByName("_override").Value));
        }

        //Ensures that CreateFormMerge behaves per expected SLA +-
        [Run]
        public void Performance()
        {
          const int CNT = 5000;

          var conf1 = Azos.Conf.XMLConfiguration.CreateFromXML(largexml1);
          var conf2 = Azos.Conf.XMLConfiguration.CreateFromXML(largexml2);

          var clock = System.Diagnostics.Stopwatch.StartNew();
          for(var i=0; i<CNT; i++)
          {
            var conf = new Azos.Conf.MemoryConfiguration();
            conf.CreateFromMerge(conf1.Root, conf2.Root);
          }
          clock.Stop();

          Console.WriteLine("Config merge performance. Merged {0} times in {1} ms", CNT, clock.ElapsedMilliseconds);

          Aver.IsTrue(clock.ElapsedMilliseconds < 7000);//completes on i7 < 1200ms for 5000 iterations
        }



static string largexml1 = @"
 <r>
    <a>
       <a-a a1='1' a2='2' a3='3' a4='4' a5='5'>
       </a-a>
    </a>

    <b>
       <a-a>
       </a-a>
    </b>

    <c _override='all'>
       <providers when='now'>
          <provider name='1' a1='1' a2='2' a3='3' a4='4' a5='5'>

          </provider>
          <provider name='2' a1='1' a2='2' a3='3' a4='4' a5='5'>

          </provider>
          <provider name='3' a1='1' a2='2' a3='3' a4='4' a5='5'>
                  <M>
                    <K a1='1' a2='2' a3='3' a4='4' a5='5'
                              b1='1' b2='2' b3='3' b4='4' b5='5'
                              c1='1' c2='2' c3='3' c4='4' c5='5'
                              >

                    </K>
                  </M>
          </provider>
          <provider name='4' a1='1' a2='2' a3='3' a4='4' a5='5'>

          </provider>
          <provider name='5' a1='1' a2='2' a3='3' a4='4' a5='5'>

          </provider>
       </providers>
    </c>

    <d>
       <a-a a1='$(/c/providers/$when)' a2='2' a3='3' a4='4' a5='5'
                              b1='1' b2='2' b3='3' b4='4' b5='5'
                              c1='1' c2='2' c3='3' c4='4' c5='5'>
       </a-a>
    </d>

    <e>
       <a-a a1='$(/c/providers/$when)' a2='2' a3='3' a4='4' a5='5'
                              b1='1' b2='2' b3='3' b4='4' b5='5'
                              c1='1' c2='2' c3='3' c4='4' c5='5'>
       </a-a>
    </e>


    <f>
       <a-a a1='$(/c/providers/$when)' a2='2' a3='3' a4='4' a5='5'
                              b1='1' b2='2' b3='3' b4='4' b5='5'
                              c1='1' c2='2' c3='3' c4='4' c5='5'>
       </a-a>
    </f>

 </r>
";

static string largexml2 = @"
 <r>
    <a>
       <a-a za1='1' za2='2' za3='3' za4='4' za5='5'>
       </a-a>
    </a>

    <b>
       <a-a>
       </a-a>
    </b>

    <c>
       <providers when='now'>
          <provider name='1' za1='1' za2='2' a3='3' a4='4' a5='5'>

          </provider>
          <provider name='2' a1='lkhsfd; adshf; lsahf ha f' za2='2' a3='3' a4='sadfsdafn.asn' a5='qwerwqer5'>

          </provider>
          <provider name='tri' a1='1' a2='2' a3='3' a4='4' a5='5'>
                  <M a='jrtglkdjfglk jdflkjg djsg jd;flkjg ;dlsfjg ;ldsfjg ldkgjd;lfgl;k jwerojgoirjg wj;erigj ;ewrigj ;weirjg ;wiergiwe;rj; ow; gjw rgjjgiuefu'>
                    <K a1='1' a2='2' a3='3' a4='4' a5='5'
                              b1='1' b2='2' >

                              hahaha!

                    </K>
                  </M>
          </provider>
          <provider name='aza' za1='1' za2='2' za3='3' za4='4' za5='5'>

          </provider>
          <provider name='zasaz' za1='1' za2='2' za3='3' za4='4' za5='5'>

          </provider>
       </providers>
    </c>

    <d>
       <a-a a1='$(/c/providers/$when)' a2='23' a3='435343' a4='ertert4' a5='ert5'
                              b1='ert1' b2='ewr2' b3='uiuyi3' b4='rettwe4' b5='ewrter5'
                              c1='1ert' c2='wer2' c3='ewrt3' c4='ert4' c5='erterter5'>
       </a-a>
    </d>

    <g>
       <mail name='$(/c/providers/$when)' a2='2' a3='3' a4='4' a5='5'>
       </mail>

       <mail name='2' a2='2' a3='3' a4='4' a5='5'>
       </mail>

       <mail name='3' a2='2' a3='3' a4='4' a5='5'>

           <demo this='will have garbage nodes'>
              <sun bright='oh yeah' animals='various'>
                <gun pattern='alalal' flexible='true' retriever='233'>
                   Some text that may take multiple lines that the way
                   we wanted to put this text in here
                </gun>
              </sun>
           </demo>

       </mail>

       <mail name='4' a2='2' a3='3' a4='4' a5='5'>
       </mail>


       <mail name='5' a2='2' a3='3' a4='4' a5='5'>
          <demo this='will have garbage nodes'>
              <sun bright='oh yeah' animals='various'>
                <gun pattern='alalal' flexible='true' retriever='233'>
                   Some text that may take multiple lines that the way
                   we wanted to put this text in here
                </gun>
              </sun>
           </demo>
           <demoA this='will have garbage nodes'>
              <sun bright='oh yeah' animals='various'>
                <gun pattern='alalal' flexible='true' retriever='233'>
                   Some text that may take multiple lines that the way
                   we wanted to put this text in here
                    <demoUnderGun this='will have garbage nodes'>
                      <sun bright='oh yeah' animals='various'>
                        <gun pattern='alalal' flexible='true' retriever='233'>
                           Some text that may take multiple lines that the way
                           we wanted to put this text in here
                        </gun>

                           <demoUnderSun this='will have garbage nodes'>
                              <sun bright='oh yeah' animals='various'>
                                <gun pattern='alalal' flexible='true' retriever='233'>
                                   Some text that may take multiple lines that the way
                                   we wanted to put this text in here
                                </gun>
                              </sun>
                           </demoUnderSun>

                      </sun>
                   </demoUnderGun>

                </gun>
              </sun>
           </demoA>
           <demoB this='will have garbage nodes'>
              <sun bright='oh yeah' animals='various'>
                <gun pattern='alalal' flexible='true' retriever='233'>
                   Some text that may take multiple lines that the way
                   we wanted to put this text in here
                </gun>
              </sun>
           </demoB>
       </mail>
    </g>

 </r>
";
        [Run]
        public void SectionMerge_1()
        {
          var conf1 = "r{ a{} c{}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
          var conf2 = "r{ b{ z = 134 }}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          conf1.OverrideBy(conf2);

          Aver.AreEqual(3, conf1.ChildCount);
          Aver.IsTrue( conf1.Navigate("/a").Exists);
          Aver.IsTrue( conf1.Navigate("/b").Exists);
          Aver.IsTrue( conf1.Navigate("/c").Exists);
          Aver.IsTrue( conf1.Navigate("/b/$z").Exists);
          Aver.AreEqual( 134,  conf1.Navigate("/b/$z").ValueAsInt());
        }

        [Run]
        public void SectionMerge_2()
        {
          var conf1 = "r{ a{} c{}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
          var conf2 = "r{ b{ z = 134 } c{ y=456} c{z=789 }}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          conf1.OverrideBy(conf2);

          Aver.AreEqual(3, conf1.ChildCount);//<---3 because all "C" get collapsed into one
          Aver.IsTrue( conf1.Navigate("/a").Exists);
          Aver.IsTrue( conf1.Navigate("/b").Exists);
          Aver.IsTrue( conf1.Navigate("/c").Exists);
          Aver.IsTrue( conf1.Navigate("/b/$z").Exists);
          Aver.AreEqual( 134,  conf1.Navigate("/b/$z").ValueAsInt());

          Aver.IsTrue( conf1.Navigate("/c/$y").Exists);
          Aver.IsTrue( conf1.Navigate("/c/$z").Exists);

          Aver.AreEqual( 456,  conf1.Navigate("/c/$y").ValueAsInt());
          Aver.AreEqual( 789,  conf1.Navigate("/c/$z").ValueAsInt());
        }

        [Run]
        public void SectionMerge_3()
        {
          var conf1 = "r{ a{} c{}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
          var conf2 = "r{ b{ z = 134 } c{ y=456} c{z=789 }}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          var rules = new NodeOverrideRules{ AppendSectionsWithoutMatchAttr = true };
          conf1.OverrideBy(conf2, rules);

          Aver.AreEqual(5, conf1.ChildCount);//<---- 5 because all "C" get appended
          Aver.IsTrue( conf1.Navigate("/a").Exists);
          Aver.IsTrue( conf1.Navigate("/b").Exists);
          Aver.IsTrue( conf1.Navigate("/c").Exists);
          Aver.IsTrue( conf1.Navigate("/b/$z").Exists);
          Aver.AreEqual( 134,  conf1.Navigate("/b/$z").ValueAsInt());

          Aver.IsTrue( conf1.Navigate("/c[y=456]").Exists);
          Aver.IsTrue( conf1.Navigate("/c[z=789]").Exists);

          Aver.AreEqual( 456,  conf1.Navigate("/c[y=456]/$y").ValueAsInt());
          Aver.AreEqual( 789,  conf1.Navigate("/c[z=789]/$z").ValueAsInt());
        }

        [Run]
        public void SectionMerge_4()
        {
          var conf1 = "r{ a{} c{}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
          var conf2 = "r{ b{ z = 134 } c{name='id1' y=456} c{name='id2' z=789 }}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          conf1.OverrideBy(conf2);

          Aver.AreEqual(5, conf1.ChildCount); //<-- 5 because names are different
          Aver.IsTrue( conf1.Navigate("/a").Exists);
          Aver.IsTrue( conf1.Navigate("/b").Exists);
          Aver.IsTrue( conf1.Navigate("/c").Exists);
          Aver.IsTrue( conf1.Navigate("/c[name=id1]").Exists);
          Aver.IsTrue( conf1.Navigate("/c[name=id2]").Exists);
          Aver.IsTrue( conf1.Navigate("/b/$z").Exists);
          Aver.AreEqual( 134,  conf1.Navigate("/b/$z").ValueAsInt());

          Aver.IsTrue( conf1.Navigate("/c[name=id1]/$y").Exists);
          Aver.IsTrue( conf1.Navigate("/c[name=id2]/$z").Exists);

          Aver.AreEqual( 456,  conf1.Navigate("/c[name=id1]/$y").ValueAsInt());
          Aver.AreEqual( 789,  conf1.Navigate("/c[name=id2]/$z").ValueAsInt());
        }

        [Run]
        public void SectionMerge_5()
        {
          var conf1 = "r{ a{} c{}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
          var conf2 = "r{ b{ z = 134 } c{name='id1' y=456} c{name='id2' z=789 } c{ gg=123}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          conf1.OverrideBy(conf2);

          Aver.AreEqual(5, conf1.ChildCount); //<-- 5 because names are different, but 6th collapses into the one without name
          Aver.IsTrue( conf1.Navigate("/a").Exists);
          Aver.IsTrue( conf1.Navigate("/b").Exists);
          Aver.IsTrue( conf1.Navigate("/c").Exists);
          Aver.IsTrue( conf1.Navigate("/c[name=id1]").Exists);
          Aver.IsTrue( conf1.Navigate("/c[name=id2]").Exists);
          Aver.IsTrue( conf1.Navigate("/b/$z").Exists);
          Aver.AreEqual( 134,  conf1.Navigate("/b/$z").ValueAsInt());

          Aver.IsTrue( conf1.Navigate("/c/$gg").Exists);
          Aver.IsTrue( conf1.Navigate("/c[name=id1]/$y").Exists);
          Aver.IsTrue( conf1.Navigate("/c[name=id2]/$z").Exists);

          Aver.AreEqual( 123,  conf1.Navigate("/c/$gg").ValueAsInt());
          Aver.AreEqual( 456,  conf1.Navigate("/c[name=id1]/$y").ValueAsInt());
          Aver.AreEqual( 789,  conf1.Navigate("/c[name=id2]/$z").ValueAsInt());
        }

        [Run]
        public void SectionMerge_6()
        {
          var conf1 = "r{ a{} c{}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);
          var conf2 = "r{ b{ z = 134 } c{name='id1' y=456} c{name='id2' z=789 } c{ gg=123}}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          var rules = new NodeOverrideRules{ AppendSectionsWithoutMatchAttr = true };
          conf1.OverrideBy(conf2, rules);

          Aver.AreEqual(6, conf1.ChildCount); //<-- 6 because names are different, but 6th gets added due to rules
          Aver.IsTrue( conf1.Navigate("/a").Exists);
          Aver.IsTrue( conf1.Navigate("/b").Exists);
          Aver.IsTrue( conf1.Navigate("/c").Exists);
          Aver.IsTrue( conf1.Navigate("/c[name=id1]").Exists);
          Aver.IsTrue( conf1.Navigate("/c[name=id2]").Exists);
          Aver.IsTrue( conf1.Navigate("/c[gg=123]").Exists);
          Aver.IsTrue( conf1.Navigate("/b/$z").Exists);
          Aver.AreEqual( 134,  conf1.Navigate("/b/$z").ValueAsInt());

          Aver.IsTrue( conf1.Navigate("/c[gg=123]/$gg").Exists);
          Aver.IsTrue( conf1.Navigate("/c[name=id1]/$y").Exists);
          Aver.IsTrue( conf1.Navigate("/c[name=id2]/$z").Exists);

          Aver.AreEqual( 123,  conf1.Navigate("/c[gg=123]/$gg").ValueAsInt());
          Aver.AreEqual( 456,  conf1.Navigate("/c[name=id1]/$y").ValueAsInt());
          Aver.AreEqual( 789,  conf1.Navigate("/c[name=id2]/$z").ValueAsInt());
        }


    }
}
