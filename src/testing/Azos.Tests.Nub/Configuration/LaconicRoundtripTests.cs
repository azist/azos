/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Conf;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class LaconicRoundtripTests
  {
    [Run]
    public void Test_01()
    {
      var c1 = LaconicConfiguration.CreateFromString("root{ a=12  b=25 }");
      var txt = c1.SaveToString();
      var c2 = LaconicConfiguration.CreateFromString(txt);

      Aver.AreEqual(2, c2.Root.AttrCount);
      Aver.AreEqual(0, c2.Root.ChildCount);
      Aver.AreEqual("12", c2.Root.AttrByName("a").Value);
      Aver.AreEqual("25", c2.Root.AttrByName("b").Value);
    }

    [Run]
    public void Test_02()
    {
      var c1 = LaconicConfiguration.CreateFromString("root{ a=000120  b=0-25 c{ d=snake} }");
      var txt = c1.SaveToString();
      var c2 = LaconicConfiguration.CreateFromString(txt);

      Aver.AreEqual(2, c2.Root.AttrCount);
      Aver.AreEqual(1, c2.Root.ChildCount);
      Aver.AreEqual("000120", c2.Root.AttrByName("a").Value);
      Aver.AreEqual("0-25", c2.Root.AttrByName("b").Value);

      Aver.AreEqual("snake", c2.Root["c"].AttrByName("d").Value);
    }

    [Run]
    public void Test_03()
    {
      var c1 = LaconicConfiguration.CreateFromString("root{ a='000 120'  b=0-25 c{ d='snake    land'} }");
      var txt = c1.SaveToString();
      var c2 = LaconicConfiguration.CreateFromString(txt);

      Aver.AreEqual(2, c2.Root.AttrCount);
      Aver.AreEqual(1, c2.Root.ChildCount);
      Aver.AreEqual("000 120", c2.Root.AttrByName("a").Value);
      Aver.AreEqual("0-25", c2.Root.AttrByName("b").Value);

      Aver.AreEqual("snake    land", c2.Root["c"].AttrByName("d").Value);
    }

    [Run]
    public void Test_04()
    {
      var c1 = LaconicConfiguration.CreateFromString("root{ a='00\u0034yes' }");
      var txt = c1.SaveToString();
      var c2 = LaconicConfiguration.CreateFromString(txt);

      Aver.AreEqual(1, c2.Root.AttrCount);
      Aver.AreEqual("00\u0034yes", c2.Root.AttrByName("a").Value);
    }

    [Run]
    public void Test_05_1()
    {
      var c1 = LaconicConfiguration.CreateFromString(@"root{ a=$'\\notcomment' }");
      var txt = c1.SaveToString();
      var c2 = LaconicConfiguration.CreateFromString(txt);

      Aver.AreEqual(1, c2.Root.AttrCount);
      Aver.AreEqual(@"\\notcomment", c2.Root.AttrByName("a").Value);
    }

    [Run]
    public void Test_05_2()
    {
      var c1 = LaconicConfiguration.CreateFromString(@"root{ a='\\\\notcomment' }");
      var txt = c1.SaveToString();
      var c2 = LaconicConfiguration.CreateFromString(txt);

      Aver.AreEqual(1, c2.Root.AttrCount);
      Aver.AreEqual(@"\\notcomment", c2.Root.AttrByName("a").Value);
    }

    [Run]
    public void Test_06()
    {
      var c1 = LaconicConfiguration.CreateFromString(@"root{ a='//notcomment' }");
      var txt = c1.SaveToString();
      var c2 = LaconicConfiguration.CreateFromString(txt);

      Aver.AreEqual(1, c2.Root.AttrCount);
      Aver.AreEqual(@"//notcomment", c2.Root.AttrByName("a").Value);
    }

    [Run]
    public void Test_07()
    {
      var c1 = LaconicConfiguration.CreateFromString(@"root{ a='/*notcomment' }");
      var txt = c1.SaveToString();
      var c2 = LaconicConfiguration.CreateFromString(txt);

      Aver.AreEqual(1, c2.Root.AttrCount);
      Aver.AreEqual(@"/*notcomment", c2.Root.AttrByName("a").Value);
    }

    [Run]
    public void Test_08()
    {
      var c1 = LaconicConfiguration.CreateFromString(@"root{ a='|*notcomment' }");
      var txt = c1.SaveToString();
      var c2 = LaconicConfiguration.CreateFromString(txt);

      Aver.AreEqual(1, c2.Root.AttrCount);
      Aver.AreEqual(@"|*notcomment", c2.Root.AttrByName("a").Value);
    }

    [Run]
    public void Test_09()
    {
      var c1 = LaconicConfiguration.CreateFromString(@"root{ a='null' }");
      var txt = c1.SaveToString();
      var c2 = LaconicConfiguration.CreateFromString(txt);

      Aver.AreEqual(1, c2.Root.AttrCount);
      Aver.AreEqual(@"null", c2.Root.AttrByName("a").Value);
    }
  }
}
