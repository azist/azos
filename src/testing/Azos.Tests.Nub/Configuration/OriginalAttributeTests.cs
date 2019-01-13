/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;
using Azos.Data;
using Azos.Scripting;
using Azos.Time;

namespace Azos.Tests.Nub.Configuration
{
    #pragma warning disable 0649,0169



        public enum MyEnum {A,B,C,D}

        [Config("MyClass/data")]
        public class MyClass
        {

           [Config("$pvt-int")]
           private int m_PrivateInt;         public int getPrivateInt(){ return m_PrivateInt;}


           private int m_privateProperty;

           [Config("$pvt-property")]
           private int privateProperty
           {
             get { return m_privateProperty;}
             set { m_privateProperty = value; }
           }   public int getPrivateProperty(){ return privateProperty; }



           [Config("$pvt-name")]
           private string m_PrivateName;         public string getPrivateName(){ return m_PrivateName;}

           [Config("$prot-name")]
           protected string m_ProtectedName;     public string getProtectedName(){ return m_ProtectedName;}

           [Config("$pub-name")]
           public string m_PublicName;

           [Config("$pub-format|$pub-def-format")]
           public string Format;

           [Config("$age")]
           public int Age;

           [Config]
           public int Age2;

           [Config]
           public int TheNewAge;

           [Config("extra/$enum")]
           public MyEnum MyEnumField;

           [Config("extra/$when")]
           public DateTime When{ get; set; }

           [Config("extra/fuzzy")]
           public bool? Fuzzy{ get; set; }

           [Config("extra/jazzy")]
           public bool? Jazzy{ get; set; }


           [Config("extra/$none1", 155)]
           public int NoneInt{ get; set; }

           [Config("extra/$none2", true)]
           public bool NoneBool{ get; set; }

           [Config("extra/$none3", "This is default")]
           public string NoneString{ get; set; }


           [Config("extra/options")]
           public IConfigSectionNode OptionsProp { get; set;}

           [Config("extra/options")]
           public IConfigSectionNode OptionsField;

           [Config]
           public byte[] Bytes;
        }


        public class MyBadClass : MyClass
        {
           [Config("$pub-format|!$NON-EXISTENT-format")]
           public string FormatThatIsRequired;
        }

        public class MyClassExtended : MyClass
        {

           [Config("$pvt-int-extended")]
           private int m_PrivateInt;         public int getPrivateIntExtended(){ return m_PrivateInt;}


           private int m_privateProperty;

           [Config("$pvt-property-extended")]
           private int privateProperty
           {
             get { return m_privateProperty;}
             set { m_privateProperty = value; }
           }   public int getPrivatePropertyExtended(){ return privateProperty; }


           //notice no attributes on this level, they will get inherited here
           [Config("abrakabadra", "So what?")]
           public string NoneAnotherString{ get; set; }
        }

        [Config("MyClassExtended2/data")]
        public class MyClassExtended2 : MyClass
        {
         //notice no attributes on this level, they will get inherited here
        }



                        [ConfigMacroContext]
                        public class SomeFatClassWithContext : ILocalizedTimeProvider, IConfigurable
                        {

                            public SomeFatClassWithContext()
                            {

                            }

                            [Config]
                            public string Text;


                            public TimeLocation TimeLocation
                            {
                                get { return new TimeLocation(new TimeSpan(2, 15, 0), "In Europe"); }
                            }

                            public DateTime LocalizedTime
                            {
                                get { return new DateTime(2000, 01, 02, 18, 30, 00); }
                            }

                            public DateTime UniversalTimeToLocalizedTime(DateTime utc)
                            {
                                return new DateTime(2000, 01, 02, 16, 15, 00);
                            }

                            public DateTime LocalizedTimeToUniversalTime(DateTime local)
                            {
                              throw new NotImplementedException();
                            }


                            public void Configure(IConfigSectionNode node)
                            {
                                Text = node.AttrByName("text").Value;
                            }


                        }




    [Runnable]
    public class OriginalAttributeTests
    {
        static string xml =
@"<root>

   <injected type='Azos.Tests.Nub.Configuration.SomeFatClassWithContext, Azos.Tests.Nub'
             text='$(::now fmt=yyyyMMdd-HHmmss)'
   />



   <MyClass>
    <data pvt-name='private'
          prot-name='protected'
          pub-name='public'
          pub-def-format='xxx'
          age='99'
          age2='890'
          the-new-age='1890'
          bytes='0,1 ,2 ,3 ,  4,5,6,7,8,9,a,b,c,d,e,f'

          pvt-int='-892'
          pvt-property='23567'


          pvt-int-extended='892'
          pvt-property-extended='-23567'

          >

          <extra
            enum='B'
            when='05/12/1982'>

            <fuzzy>true</fuzzy>
            <jazzy></jazzy>

            <options>
                <hello a='1'>YES!</hello>
            </options>

          </extra>


    </data>
  </MyClass>

  <MyClassExtended2>
    <data prot-name='protected'
          pub-name='public'
          age='199'>

          <extra
            enum='C'
            when='01/1/1944'>

            <fuzzy>false</fuzzy>
            <jazzy></jazzy>

          </extra>


    </data>
  </MyClassExtended2>
 </root>
";



        [Run]
        public void ConfigAttributeApply()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyClass();
          ConfigAttribute.Apply(cl, conf.Root);

          Aver.AreEqual("private", cl.getPrivateName());
          Aver.AreEqual("protected", cl.getProtectedName());
          Aver.AreEqual("public", cl.m_PublicName);


           Aver.AreEqual(-892, cl.getPrivateInt());
           Aver.AreEqual(23567, cl.getPrivateProperty());

          Aver.AreEqual("xxx", cl.Format);

          Aver.AreEqual(99, cl.Age);
          Aver.IsTrue( MyEnum.B == cl.MyEnumField);

          Aver.AreEqual(5, cl.When.Month);
          Aver.AreEqual(12, cl.When.Day);
          Aver.AreEqual(1982, cl.When.Year);

          Aver.AreEqual(true, cl.Fuzzy.Value);

          Aver.AreEqual(false, cl.Jazzy.HasValue);

           Aver.AreEqual(155, cl.NoneInt);
           Aver.AreEqual(true, cl.NoneBool);
           Aver.AreEqual("This is default", cl.NoneString);
        }

        [Run]
        public void ConfigAttributeWithoutPath()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyClass();
          ConfigAttribute.Apply(cl, conf.Root);

          Aver.AreEqual(99, cl.Age);
          Aver.AreEqual(890, cl.Age2);
          Aver.AreEqual(1890, cl.TheNewAge);
        }

         [Run]
        public void ConfigAttributeByteArray()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyClass();
          ConfigAttribute.Apply(cl, conf.Root);

          var bytes =  cl.Bytes;
          Aver.AreEqual(16, bytes.Length);

          Aver.AreEqual(0, bytes[00]);
          Aver.AreEqual(1, bytes[01]);
          Aver.AreEqual(2, bytes[02]);
          Aver.AreEqual(3, bytes[03]);
          Aver.AreEqual(4, bytes[04]);
          Aver.AreEqual(5, bytes[05]);
          Aver.AreEqual(6, bytes[06]);
          Aver.AreEqual(7, bytes[07]);
          Aver.AreEqual(8, bytes[08]);
          Aver.AreEqual(9, bytes[09]);
          Aver.AreEqual(0xa, bytes[10]);
          Aver.AreEqual(0xb, bytes[11]);
          Aver.AreEqual(0xc, bytes[12]);
          Aver.AreEqual(0xd, bytes[13]);
          Aver.AreEqual(0xe, bytes[14]);
          Aver.AreEqual(0xf, bytes[15]);
        }


        [Run]
        [Aver.Throws(typeof(ConfigException))]
        public void ConfigAttributeApplyToNonExistingRequiredCoalescedAttribute()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyBadClass();
          ConfigAttribute.Apply(cl, conf.Root);
        }



        [Run]
        public void ConfigAttributeApplyToICongifSectionPropertyAndField()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyClass();
          ConfigAttribute.Apply(cl, conf.Root);


           Aver.AreEqual("YES!", cl.OptionsProp["hello"].Value);
           Aver.AreEqual(1, cl.OptionsProp["hello"].AttrByName("a").ValueAsInt());

           Aver.AreEqual("YES!", cl.OptionsField["hello"].Value);
           Aver.AreEqual(1, cl.OptionsField["hello"].AttrByName("a").ValueAsInt());
        }


        [Run]
        public void ConfigAttributeApplyToExtendedClass()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyClassExtended();
          ConfigAttribute.Apply(cl, conf.Root);

          Aver.AreEqual("private", cl.getPrivateName());
          Aver.AreEqual("protected", cl.getProtectedName());
          Aver.AreEqual("public", cl.m_PublicName);


          Aver.AreEqual(-892, cl.getPrivateInt());
          Aver.AreEqual(23567, cl.getPrivateProperty());

          Aver.AreEqual(+892, cl.getPrivateIntExtended());
          Aver.AreEqual(-23567, cl.getPrivatePropertyExtended());


          Aver.AreEqual(99, cl.Age);
          Aver.IsTrue(MyEnum.B == cl.MyEnumField);

          Aver.AreEqual(5, cl.When.Month);
          Aver.AreEqual(12, cl.When.Day);
          Aver.AreEqual(1982, cl.When.Year);

          Aver.AreEqual(true, cl.Fuzzy.Value);

          Aver.AreEqual(false, cl.Jazzy.HasValue);

           Aver.AreEqual(155, cl.NoneInt);
           Aver.AreEqual(true, cl.NoneBool);
           Aver.AreEqual("This is default", cl.NoneString);
           Aver.AreEqual("So what?", cl.NoneAnotherString);

        }

        [Run]
        public void ConfigAttributeApplyToExtendedClassWithRootOverride()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          var cl = new MyClassExtended2();
          ConfigAttribute.Apply(cl, conf.Root);

          Aver.AreEqual("protected", cl.getProtectedName());
          Aver.AreEqual("public", cl.m_PublicName);

          Aver.AreEqual(199, cl.Age);
          Aver.IsTrue(MyEnum.C == cl.MyEnumField);

          Aver.AreEqual(1, cl.When.Month);
          Aver.AreEqual(1, cl.When.Day);
          Aver.AreEqual(1944, cl.When.Year);

          Aver.AreEqual(false, cl.Fuzzy.Value);

          Aver.AreEqual(false, cl.Jazzy.HasValue);


        }

        [Run]
        public void MakeWithMacroContextAndLocalizedTime()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          var node = conf.Root["injected"];
          var obj = FactoryUtils.Make<SomeFatClassWithContext>(node);

          ConfigAttribute.Apply(obj, node);

          Aver.AreEqual("20000102-183000", obj.Text);
        }

        [Run]
        public void MakeAndConfigureWithMacroContextAndLocalizedTime()
        {
          var conf = Azos.Conf.XMLConfiguration.CreateFromXML(xml);

          var node = conf.Root["injected"];
          var obj = FactoryUtils.MakeAndConfigure<SomeFatClassWithContext>(node);

          Aver.AreEqual("20000102-183000", obj.Text);
        }


        private class TeztClazz
        {
          [Config(Verbatim=true)]   public string Verbatim{ get; set;}
          [Config]                  public string Evaluated{ get; set;}
        }

        [Run]
        public void ConfigAttrApplyVerbatimAndEvaluated()
        {
          var conf = "r{  a=234   b=$($a)  verbatim=$(sub/$d) evaluated=$(sub/$d) sub{ d=$(/$b)} }".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

          var obj = new TeztClazz();
          ConfigAttribute.Apply(obj, conf);

          Aver.AreEqual( "$(sub/$d)" , obj.Verbatim );
          Aver.AreEqual( "234" ,       obj.Evaluated );
        }
    }
}



