using System;
using System.Collections.Generic;
using System.Text;

using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.Serialization.JSON.Backends;
using Azos.Text;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JazonReadTests
  {
    private static readonly IJsonReaderBackend JZ = new JazonReaderBackend();


    [Run]
    public void Integers()
    {
      var map = JZ.DeserializeFromJson("{ a: 1, b: -1, c: +2, d: -2, e:  2147483647, f: -2147483647}", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(6, map.Count);
      Aver.AreObjectsEqual(1, map["a"]);
      Aver.AreObjectsEqual(-1, map["b"]);
      Aver.AreObjectsEqual(2, map["c"]);
      Aver.AreObjectsEqual(-2, map["d"]);
      Aver.AreObjectsEqual(2_147_483_647, map["e"]);
      Aver.AreObjectsEqual(-2_147_483_647, map["f"]);
    }

    [Run]
    public void Longs()
    {
      var map = JZ.DeserializeFromJson("{ a: 10000000000, b: -10000000000, c: +20000000000, d: -20000000000, e:  9223372036854775807, f: -9223372036854775807}", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(6, map.Count);
      Aver.AreObjectsEqual(10000000000, map["a"]);
      Aver.AreObjectsEqual(-10000000000, map["b"]);
      Aver.AreObjectsEqual(20_000_000_000, map["c"]);
      Aver.AreObjectsEqual(-20_000_000_000, map["d"]);
      Aver.AreObjectsEqual(9_223_372_036_854_775_807, map["e"]);
      Aver.AreObjectsEqual(-9_223_372_036_854_775_807, map["f"]);
    }

    [Run]
    public void Doubles()
    {
      var map = JZ.DeserializeFromJson("{ pi: 3.14159265359, exp1: 123e4, exp2: 2e-5, exp3: 2e+3, exp4: -0.2e+2 }", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(5, map.Count);
      Aver.AreObjectsEqual(3.14159265359D, map["pi"]);
      Aver.AreObjectsEqual(123e4D, map["exp1"]);
      Aver.AreObjectsEqual(2e-5D, map["exp2"]);
      Aver.AreObjectsEqual(2e+3D, map["exp3"]);
      Aver.AreObjectsEqual(-0.2e+2D, map["exp4"]);
    }

    [Run]
    public void Bools()
    {
      var map = JZ.DeserializeFromJson("{ a: true, b: false, c: null}", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(3, map.Count);
      Aver.AreObjectsEqual(true, map["a"]);
      Aver.AreObjectsEqual(false, map["b"]);
      Aver.AreObjectsEqual(null, map["c"]);
    }

    [Run]
    public void Strings_Basic()
    {
      var map = JZ.DeserializeFromJson("{ a: 'abc', b: \"def's\", c: 'with\\nescapes\\r'}", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(3, map.Count);
      Aver.AreObjectsEqual("abc", map["a"]);
      Aver.AreObjectsEqual("def's", map["b"]);
      Aver.AreObjectsEqual("with\nescapes\r", map["c"]);
    }

    [Run]
    public void Strings_Verbatim_1()
    {
      var map = JZ.DeserializeFromJson(@"{ a: $'
      this string
      spans many lines
      and uses \escapes that \donot \ufff??? evaluate
      for Mc''Cloud''s ""name""
      thats all
      '}", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(1, map.Count);
      Aver.IsTrue(map["a"].ToString().MatchPattern(@"*this string* uses \escapes that \donot \ufff??? evaluate*Mc'Cloud's ""name""*all*"));
    }

    [Run]
    public void Strings_Verbatim_2()
    {
      var map = JZ.DeserializeFromJson(@"{ a: $""
      this string
      spans many lines
      and uses \escapes that \donot \ufff??? evaluate
      for Mc'Cloud's """"name""""
      thats all
      ""}", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(1, map.Count);
      Aver.IsTrue(map["a"].ToString().MatchPattern(@"*this string* uses \escapes that \donot \ufff??? evaluate*Mc'Cloud's ""name""*all*"));
    }

  }
}
