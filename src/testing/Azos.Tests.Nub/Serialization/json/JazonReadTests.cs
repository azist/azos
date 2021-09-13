/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
    public void Object_1()
    {
      var map = JZ.DeserializeFromJson("{ a: 1, b: 2, c: null, d: {a: 'string'}, e: [ - 1000,null, true, -2e3,{msg: 'my message'}]}", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(5, map.Count);
      Aver.AreObjectsEqual(1, map["a"]);
      Aver.AreObjectsEqual(2, map["b"]);
      Aver.AreObjectsEqual(null, map["c"]);

      var d = map["d"] as JsonDataMap;
      Aver.IsNotNull(d);
      Aver.AreObjectsEqual("string", d["a"]);

      var e = map["e"] as JsonDataArray;
      Aver.IsNotNull(e);
      Aver.AreEqual(5, e.Count);

      Aver.AreObjectsEqual(-1000, e[0]);
      Aver.AreObjectsEqual(null, e[1]);
      Aver.AreObjectsEqual(true, e[2]);
      Aver.AreObjectsEqual(-2e3, e[3]);

      var e5 = e[4] as JsonDataMap;
      Aver.IsNotNull(e5);
      Aver.AreEqual(1, e5.Count);
      Aver.AreObjectsEqual("my message", e5["msg"]);
    }

    [Run]
    public void Object_2()
    {
      var got = JZ.DeserializeFromJson("[1,2,3]", true) as JsonDataArray;
      Aver.IsNotNull(got);
      Aver.AreEqual(3, got.Count);
      Aver.AreObjectsEqual(1, got[0]);
      Aver.AreObjectsEqual(2, got[1]);
      Aver.AreObjectsEqual(3, got[2]);
    }

    [Run]
    public void Object_3()
    {
      var got = JZ.DeserializeFromJson("123", true);
      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(123, got);
    }

    [Run]
    public void Object_4()
    {
      var got = JZ.DeserializeFromJson("-123", true);
      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(-123, got);
    }

    [Run]
    public void Object_5()
    {
      var got = JZ.DeserializeFromJson("-123000000000", true);
      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(-123_000_000_000, got);
    }

    [Run]
    public void Object_6()
    {
      var got = JZ.DeserializeFromJson("true", true);
      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(true, got);
    }

    [Run]
    public void Object_7()
    {
      var got = JZ.DeserializeFromJson("false", true);
      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(false, got);
    }

    [Run]
    public void Object_8()
    {
      var got = JZ.DeserializeFromJson("null", true);
      Aver.IsNull(got);
    }

    [Run]
    public void Object_9()
    {
      var got = JZ.DeserializeFromJson("'string'", true);
      Aver.IsNotNull(got);
      Aver.AreObjectsEqual("string", got);
    }

    [Run]
    public void Object_10()
    {
      var got = JZ.DeserializeFromJson("{}", true) as JsonDataMap;
      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(0, got.Count);
    }

    [Run]
    public void Object_11()
    {
      var got = JZ.DeserializeFromJson("[]", true) as JsonDataArray;
      Aver.IsNotNull(got);
      Aver.AreObjectsEqual(0, got.Count);
    }

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

    [Run]
    public void Comments_1()
    {
      var map = JZ.DeserializeFromJson(@"{ 
       a: 1, //comment1
      /* comment2 */
       b: 2  // comment /* 3*/
       ,/**/c:/* */3
      }", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(3, map.Count);
      Aver.AreObjectsEqual(1, map["a"]);
      Aver.AreObjectsEqual(2, map["b"]);
      Aver.AreObjectsEqual(3, map["c"]);
    }

    [Run]
    public void Comments_2()
    {
      var map = JZ.DeserializeFromJson(@"{ 
       a: 1, //comment1
      /* ""comment2 */
       b: 2  // commen't /* 3*/
       ,/**/c: ""/* */3""
      }", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(3, map.Count);
      Aver.AreObjectsEqual(1, map["a"]);
      Aver.AreObjectsEqual(2, map["b"]);
      Aver.AreObjectsEqual("/* */3", map["c"]);
    }

    [Run]
    public void EmptyStrings_1()
    {
      var map = JZ.DeserializeFromJson(@"{ a: ''}", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(1, map.Count);
      Aver.AreObjectsEqual("", map["a"]);
    }

    [Run]
    public void EmptyStrings_2()
    {
      var map = JZ.DeserializeFromJson(@"{ a: '', b: """"}", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(2, map.Count);
      Aver.AreObjectsEqual("", map["a"]);
      Aver.AreObjectsEqual("", map["b"]);
    }

    [Run]
    public void EmptyStrings_3()
    {
      var map = JZ.DeserializeFromJson(@"{ a: '',b: """", c: $""

      ""}", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(3, map.Count);
      Aver.AreObjectsEqual("", map["a"]);
      Aver.AreObjectsEqual("", map["b"]);
      Aver.IsTrue(map["c"].ToString().IsNullOrWhiteSpace());
    }

    [Run]
    public void EmptyStrings_4()
    {
      var map = JZ.DeserializeFromJson(@"{ a: '', b: '\n\n\r\u3456 abc',c: """" }", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(3, map.Count);
      Aver.AreObjectsEqual("", map["a"]);
      Aver.AreObjectsEqual("\n\n\r\u3456 abc", map["b"]);
      Aver.IsTrue(map["c"].ToString().IsNullOrWhiteSpace());
    }

    [Run]
    public void EmptyStrings_5()
    {
      var map = JZ.DeserializeFromJson(@"{ a: '', b: $'\n\n\r\u3456 abc',c: """" }", true) as JsonDataMap;
      Aver.IsNotNull(map);
      Aver.AreEqual(3, map.Count);
      Aver.AreObjectsEqual("", map["a"]);
      Aver.AreObjectsEqual(@"\n\n\r\u3456 abc", map["b"]);
      Aver.IsTrue(map["c"].ToString().IsNullOrWhiteSpace());
    }

  }
}
