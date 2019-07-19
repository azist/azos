/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Scripting;
using Azos.Conf;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class RunnerTests
  {
      [Run(order: -1)]
      //[Run("category", "name", -1, "")]
      public void Empty(){  Aver.Pass();  }

      [Run(@"message=$'
    This prints
    a multiline message.
    Thats all!
      '")]
      public void PrintMessage()
      {

      }

      [Run]
      public void SeeConsoleDump()
      {
        "Message without parameters".See();
        "Message with parameter {0}".See(1);
        new {a=1,b=2}.See();
        new { a = 1, b = 2 }.See("With header");
        new []{1,2,3}.See();
        "Info text".Info();
        "Info text line".Info(5);

        "Warning text".Warning();
        "Warning text line".Warning(5);

        "Error text".Error();
        "Error text line".Error(5);
      }

      [Run, Aver.Throws(typeof(NotSupportedException))]
      public void Throws1(){  throw new NotSupportedException();  }

      [Run, Aver.Throws(typeof(NotSupportedException), "bad")]
      public void Throws2(){  throw new NotSupportedException("today is a bad day");  }

      [Run, Aver.Throws(typeof(NotSupportedException), ExactType = true, Message = "bad", MsgMatch = Aver.ThrowsAttribute.MatchType.Exact)]
      public void Throws3(){  throw new NotSupportedException("bad");  }

      [Run, Aver.Throws(typeof(Exception), ExactType = false)]//thrown sub-type
      public void Throws4(){  throw new NotSupportedException();  }

      [Run("a=123")]
      public void Int1(int a){  Aver.AreEqual(123, a);  }

      [Run("@{a=123}")]
      public void Int2(int a){  Aver.AreEqual(123, a);  }

      [Run("something=yes @{a=123}")]
      public void Int3(int a){  Aver.AreEqual(123, a);  }

      [Run("a=1 b=-1 c=2")]
      public void Int4(int a, int b, int c)
      {
        Aver.AreEqual(1, a);
        Aver.AreEqual(-1, b);
        Aver.AreEqual(2, c);
      }

      [Run("a=1 b=-1 c=2")]
      public void Int5(int a, int b, int c=890)
      {
        Aver.AreEqual(1, a);
        Aver.AreEqual(-1, b);
        Aver.AreEqual(2, c);
      }

      [Run("a=1 b=-1 ")]
      public void Int6(int a, int b, int c=890)
      {
        Aver.AreEqual(1, a);
        Aver.AreEqual(-1, b);
        Aver.AreEqual(890, c);
      }

      [Run("a=1 b=-1 sect{a=100 b=200}")]
      public void Section(int a, int b, IConfigSectionNode sect)
      {
        Aver.AreEqual(1, a);
        Aver.AreEqual(-1, b);

        Aver.AreEqual(100, sect.AttrByName("a").ValueAsInt());
        Aver.AreEqual(200, sect.AttrByName("b").ValueAsInt());
      }

      [Run("by=23 sby=-90 i=8394 ui=39403 s=-344 us=34324 f=2.03 dbl=0.00002 l=-32423423 ul=24234234 dec=2.00032 b=true dt='May 1, 2017' str='Zhaba'")]
      public void Types(byte    by,
                        sbyte   sby,
                        int     i,
                        uint    ui,
                        short   s,
                        ushort  us,
                        float   f,
                        double  dbl,
                        long    l,
                        ulong   ul,
                        decimal dec,
                        bool     b,
                        DateTime dt,
                        string   str)
      {
        Aver.AreEqual(23, by);
        Aver.AreEqual(-90, sby);
        Aver.AreEqual(8394, i);
        Aver.AreEqual((uint)39403, ui);
        Aver.AreEqual(-344, s);
        Aver.AreEqual(34324, us);
        Aver.AreEqual(2.03f, f);
        Aver.AreEqual(0.00002d, dbl);
        Aver.AreEqual(-32423423L, l);
        Aver.AreEqual(24234234ul, ul);
        Aver.AreEqual(2.00032m, dec);
        Aver.AreEqual(true, b);
        Aver.AreEqual(new DateTime(2017, 5, 1), dt);
        Aver.AreEqual("Zhaba", str);
      }


      [Run(null, 1, "a=1 b=1")]
      [Run(null, 2, "a=2 b=2")]
      [Run(null, 3, "a=3 b=3")]
      [Run(null, 4, "a=4 b=4")]
      [Run(null, 5, "a=5 b=5")]
      [Run(null, 9, "a=6 b=6")]
      [Run(null, 8, "a=7 b=7")]
      [Run(null, 7, "a=8 b=8")]
      [Run(null, 6, "a=9 b=9")]
      public void Multiple(int a, int b)
      {
        Aver.AreEqual(a, b);
      }

      [Run("!throws1",""),Run("!throws2",""),Run("!throws3",""),Run("!throws4",""),Run("!throws5",""),Run("!throws6",""),Run("!throws7"," bad config ")]
      public void ThrowingException()
      {
        throw new Exception("This is meant to be thrown!");
      }

      [Run("a{ x=10 y=20 log{ good=true }}")]
      public void InjectConfig_IConfigSectionNode(int x, int y, IConfigSectionNode log)
      {
        Aver.AreEqual(10, x);
        Aver.AreEqual(20, y);
        Aver.IsNotNull(log);
        Aver.IsTrue( log is ConfigSectionNode );
        Aver.AreEqual(1, log.AttrCount );
        Aver.IsTrue( log.AttrByName("good").ValueAsBool(false) );
      }

      [Run("a{ x=15 y=25 log{ good=true k=23.78} }")]
      public void InjectConfig_ConfigSectionNode(int x, int y, ConfigSectionNode log)
      {
        Aver.AreEqual(15, x);
        Aver.AreEqual(25, y);
        Aver.IsNotNull(log);
        Aver.AreEqual(2, log.AttrCount );
        Aver.IsTrue( log.AttrByName("good").ValueAsBool(false) );
        Aver.AreEqual( 23.78f,  log.AttrByName("k").ValueAsFloat() );
      }

      public class Person : IConfigurable
      {
        [Config] public string Name { get; set;}
        [Config] public int Age { get; set;}
        public void Configure(IConfigSectionNode node) => ConfigAttribute.Apply(this, node);
      }

      [Run("case1", "a{ expectName='aaa' expectAge=-125 person{ name='aaa' age=-125}}")]
      [Run("case2", "expectName='kozel' expectAge=125 person{ name='kozel' age=125}")]
      public void InjectComplexType(string expectName, int expectAge, Person person)
      {
        Aver.IsNotNull(person);
        Aver.AreEqual(expectName, person.Name);
        Aver.AreEqual(expectAge, person.Age);
      }

      //notice: this carse will run ONLY if 'names=.....' is specified e.g. 'names=cli-*' would match
      [Run("!cli-args", "cliArg1=$(~@a1) cliArg2=$(~@a2)")]
      public void RunnerArgs(string cliArg1, int cliArg2)
      {
        Aver.AreEqual("Abc", cliArg1, "Did you forget to pass - r args='a1=Abc a2=149' ?");
        Aver.AreEqual(149, cliArg2);
      }


      [Run]
      public void Arrays_1D_arrays()
      {
        Array a1 = null;
        Array a2 = null;
        Aver.AreArraysEquivalent(a1, a2);

        a1 = new int[]{1, 2, 3, 4, 5};
        a2 = new int[]{1, 2, 3, 4, 5};
        Aver.AreArraysEquivalent(a1, a2);
      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException))]
                      public void Arrays_1D_arrays_throws()
                      {
                        Array a1 = new int[]{1, 2, 3, 4, 5};
                        Array a2 = new int[]{1, -2, 3, 4, 5};
                        Aver.AreArraysEquivalent(a1, a2);
                      }

      [Run]
      public void Arrays_1D_T()
      {
        int[] a1 = null;
        int[] a2 = null;
        Aver.AreArraysEquivalent(a1, a2);

        a1 = new int[]{1, 2, 3, 4, 5};
        a2 = new int[]{1, 2, 3, 4, 5};
        Aver.AreArraysEquivalent(a1, a2);
      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException), "index 2")]
                      public void Arrays_1D_T_throws_1()
                      {
                        var a1 = new int[]{1, 2, 3, 4, 5};
                        var a2 = new int[]{1, 2, -3, 4, 5};
                        Aver.AreArraysEquivalent(a1, a2);
                      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException), "index 1")]
                      public void Arrays_1D_T_throws_2()
                      {
                        var a1 = new int[]{1, 2, 3, 4, 5};
                        var a2 = new int[]{1, 0, -3, 4, 5};
                        Aver.AreArraysEquivalent(a1, a2);
                      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException), "index -1")]
                      public void Arrays_1D_T_throws_3()
                      {
                        var a1 = new int[]{1, 2, 3, 4, 5};
                        var a2 = new int[]{1, 2, -3, 4, 5, 7, 8, 9, 0, 1};
                        Aver.AreArraysEquivalent(a1, a2);
                      }

      [Run]
      public void Arrays_1D_Nullable_T()
      {
        int?[] a1 = null;
        int?[] a2 = null;
        Aver.AreArraysEquivalent(a1, a2);

        a1 = new int?[]{1, 2, null, 4, null};
        a2 = new int?[]{1, 2, null, 4, null};
        Aver.AreArraysEquivalent(a1, a2);
      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException), "index 3")]
                      public void Arrays_1D_Nullable_T_throws_1()
                      {
                        var a1 = new int?[]{1, 2, null, 4, null};
                        var a2 = new int?[]{1, 2, null, null, null};
                        Aver.AreArraysEquivalent(a1, a2);
                      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException), "index 0")]
                      public void Arrays_1D_Nullable_T_throws_2()
                      {
                        var a1 = new int?[]{1, 2, null, 4, null};
                        var a2 = new int?[]{-1, 2, null, 4, null};
                        Aver.AreArraysEquivalent(a1, a2);
                      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException), "index -1")]
                      public void Arrays_1D_Nullable_T_throws_3()
                      {
                        var a1 = new int?[]{1, 2, null, 4, null};
                        var a2 = new int?[]{1, 2, null, 4, null, null, 1, 2};
                        Aver.AreArraysEquivalent(a1, a2);
                      }

      [Run]
      public void Arrays_1D_Objects()
      {
        string[] a1 = null;
        string[] a2 = null;
        Aver.AreArraysEquivalent(a1, a2);

        a1 = new string[]{"a", "b", null};
        a2 = new string[]{"a", "b", null};
        Aver.AreArrayObjectsEquivalent(a1, a2);
      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException), "index 0")]
                      public void Arrays_1D_Objects_throws1()
                      {
                        var a1 = new string[]{"a", "b", null};
                        var a2 = new string[]{"z", "b", null};
                        Aver.AreArrayObjectsEquivalent(a1, a2);
                      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException), "index 0")]
                      public void Arrays_1D_Objects_throws2()
                      {
                        var a1 = new string[]{"a", "b", null};
                        var a2 = new string[]{null, "b", null};
                        Aver.AreArrayObjectsEquivalent(a1, a2);
                      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException), "index -1")]
                      public void Arrays_1D_Objects_throws3()
                      {
                        var a1 = new string[]{"a", "b", null};
                        var a2 = new string[]{"a", "b", null, null, null};
                        Aver.AreArrayObjectsEquivalent(a1, a2);
                      }

      [Run]
      public void Arrays_2D()
      {
        int[,] a1 = null;
        int[,] a2 = null;
        Aver.AreArraysEquivalent(a1, a2);

        a1 = new int[,]{ {1,2,3,4, 5}, {-1,-2,-3,-4, -5} };
        a2 = new int[,]{ {1,2,3,4, 5}, {-1,-2,-3,-4, -5} };
        Aver.AreArraysEquivalent(a1, a2);
      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException))]
                      public void Arrays_2D_throws_1()
                      {
                        var a1 = new int[,]{ {1,2,0,4, 5}, {-1,-2,-3,-4, -5} };
                        var a2 = new int[,]{ {1,2,3,4, 5}, {-1,-2,-3,-4, -5} };
                        Aver.AreArraysEquivalent(a1, a2);
                      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException))]
                      public void Arrays_2D_throws_2()
                      {
                        var a1 = new int[,]{ {1,2,3,4, 5, 4}, {-1,-2,-3,-4, -5, -7} };
                        var a2 = new int[,]{ {1,2,3,4, 5}, {-1,-2,-3,-4, -5} };
                        Aver.AreArraysEquivalent(a1, a2);
                      }

      [Run]
      public void Arrays_3D()
      {
        int[,,] a1 = null;
        int[,,] a2 = null;
        Aver.AreArraysEquivalent(a1, a2);

        a1 = new int[,,]{ {{1,2,3,4, 5}, {-1,-2,-3,-4, -5}, {-10,-20,-30,-40, -50} }};
        a2 = new int[,,]{ {{1,2,3,4, 5}, {-1,-2,-3,-4, -5}, {-10,-20,-30,-40, -50} }};
        Aver.AreArraysEquivalent(a1, a2);
      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException))]
                      public void Arrays_3D_throws_1()
                      {
                        var a1 = new int[,,]{ {{1,2,3,4, 5}, {-1,-2,-3,-4, -5}, {-10,-20,-30,-40, -50} }};
                        var a2 = new int[,,]{ {{1,2,3,4, 5}, {-1,-2,-3,234, -5}, {-10,-20,-30,-40, -50} }};
                        Aver.AreArraysEquivalent(a1, a2);
                      }

                      [Run]
                      [Aver.Throws(typeof(AvermentException))]
                      public void Arrays_3D_throws_2()
                      {
                        var a1 = new int[,,]{{ {1,2}, {-1,-2}, {-10,-20} }};
                        var a2 = new int[,,]{ {{1,2,3,4, 5}, {-1,-2,-3,234, -5}, {-10,-20,-30,-40, -50} }};
                        Aver.AreArraysEquivalent(a1, a2);
                      }

  }
}
