/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.CodeAnalysis.Source;
using Azos.Serialization.JSON.Backends;
using Azos.CodeAnalysis.JSON;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JazonParserTests
  {
    [Run]
    public void ParserTest()
    {
      var json = @"{ a:       1, b: ""something"", c: null, d: {}, e: 23.7}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true);

      got.See();

    }

    [Run]
    public void ULongMax()
    {
      var json = @"{ v: 18446744073709551615}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(ulong.MaxValue, got["v"]);

    }

    [Run]
    public void Long_1()
    {
      var json = @"{ v: -9223372036854775808}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(long.MinValue, got["v"]);

    }

    [Run]
    public void Long_2()
    {
      var json = @"{ v: 9223372036854775807}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(long.MaxValue, got["v"]);

    }

    [Run]
    public void Long_3()
    {
      var json = @"{ v: +9223372036854775807}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(long.MaxValue, got["v"]);

    }

    [Run]
    public void Int_1()
    {
      var json = @"{ v: -2147483647}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(int.MinValue+1, got["v"]);//+1 because lexer treats int as [-2147483647, +2147483647] whereas int min is -214748364[8]
    }

    [Run]
    public void Int_2()
    {
      var json = @"{ v: 2147483647}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(int.MaxValue, got["v"]);

    }

    [Run]
    public void Int_3()
    {
      var json = @"{ v: +2147483647}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(int.MaxValue, got["v"]);

    }

    [Run]
    [Aver.Throws(typeof(JazonDeserializationException), Message = "eSyntaxError")]
    public void Int_4()
    {
      var json = @"{ v: ++1}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;
    }

    [Run]
    [Aver.Throws(typeof(JazonDeserializationException), Message = "eSyntaxError")]
    public void Int_5()
    {
      var json = @"{ v: --1}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;
    }

    [Run]
    [Aver.Throws(typeof(JazonDeserializationException), Message = "eValueTooBig")]
    public void Int_6()
    {
      var json = @"{ v: 687346857632847659872364785623480580392805982312348927318497293749826346213786482376}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;
    }


    [Run]
    public void Dbl_1()
    {
      var json = @"{ v: 2.12}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(2.12d, got["v"]);
    }

    [Run]
    public void Dbl_2()
    {
      var json = @"{ v: +2.12}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(2.12d, got["v"]);
    }

    [Run]
    public void Dbl_3()
    {
      var json = @"{ v: -2.12}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(-2.12d, got["v"]);
    }

    [Run]
    public void Dbl_4()
    {
      var json = @"{ v: -2e3}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(-2e3, got["v"]);
    }

    [Run]
    public void Dbl_5()
    {
      var json = @"{ v: -2.2e3}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(-2.2e3, got["v"]);
    }

    [Run]
    [Aver.Throws(typeof(JazonDeserializationException), Message = "eSyntaxError")]
    public void Dbl_6()
    {
      var json = @"{ v: --2.2e3}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;
    }

    [Run]
    [Aver.Throws(typeof(JazonDeserializationException), Message = "eSyntaxError")]
    public void Dbl_7()
    {
      var json = @"{ v: ++2.2e3}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;
    }

    [Run]
    [Aver.Throws(typeof(JazonDeserializationException), Message = "eValueTooBig")]
    public void Dbl_8()
    {
      var json = @"{ v: 6873468576328476598723647856234805803928059823123489273184972937498263462137864823.76}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;
    }


    [Run]
    public void Depth_1()
    {
      var json = @"{ v: 'abc'}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual("abc", got["v"]);
    }

    [Run]
    public void Depth_2()
    {
      var json = @"{ v: 'abc'}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true, 1) as JsonDataMap;

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual("abc", got["v"]);
    }

    [Run]
    [Aver.Throws(typeof(JazonDeserializationException), Message = "eGraphDepthLimit")]
    public void Depth_3()
    {
      var json = @"{ v: 'abc'}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true, 0) as JsonDataMap;
    }

    [Run]
    public void Depth_4()
    {
      var json = @"{ v: {v2: 'abc'}}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true, 2) as JsonDataMap;

      var v = got["v"] as JsonDataMap;
      Aver.IsNotNull(v);
      Aver.AreObjectsEqual("abc", v["v2"]);
    }

    [Run]
    [Aver.Throws(typeof(JazonDeserializationException), Message = "eGraphDepthLimit")]
    public void Depth_5()
    {
      var json = @"{ v: {v2: 'abc'}}";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true, 1) as JsonDataMap;
    }


    [Run]
    public void Depth_6()
    {
      var json = @"123";
      var src = new StringSource(json);
      var got = JazonParser.Parse(src, true, 0);//only allow root values, not objects or arrays

      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(123, got);
    }

  }
}

