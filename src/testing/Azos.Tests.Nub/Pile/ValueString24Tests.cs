/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Diagnostics;

using Azos.Scripting;
using VALS = Azos.Pile.ValueString24;

namespace Azos.Tests.Nub.Pile
{
  [Runnable]
  public class ValueString24Tests
  {
    private string[] a1;
    private VALS[] a2;
    private Complex[] a3;

    public struct Complex
    {
      public Complex(VALS s1, VALS s2)
      {
        S1 =s1; S2=s2;
        Int1=0; Int2=190;
      }
      public readonly int Int1;
      public readonly VALS S1;
      public readonly int Int2;
      public readonly VALS S2;
    }

    [Run]
    public void Basic()
    {
      VALS vs = new VALS("123456789012345678901234");
      var vs2 = new VALS("123456789012345678901234");

      System.Runtime.InteropServices.Marshal.SizeOf<VALS>().See();

      Aver.AreEqual("123456789012345678901234", vs.ToString());
    }

    [Run]
    public void Benchmark()
    {
      var a = new VALS[100];
      System.Runtime.InteropServices.Marshal.SizeOf<VALS>().See();

      var sw = Stopwatch.StartNew();

      a1 = new string[16 * 1024 * 1024];
      for(var i=0; i< a1.Length; i++)
       a1[i] = new string(' ', 16);

      var e1 = sw.ElapsedMilliseconds;
      sw.Restart();

      a2 = new VALS[16 * 1024 * 1024];
      for (var i = 0; i < a2.Length; i++)
        a2[i] = new VALS("1234567890123456");//4567890123456");

      var e2 = sw.ElapsedMilliseconds;

      sw.Restart();

      a3 = new Complex[8 * 1024 * 1024];
      for (var i = 0; i < a3.Length; i++)
        a3[i] = new Complex(new VALS("123"), new VALS("1234567890123456"));

      var e3 = sw.ElapsedMilliseconds;
      "Allocated string[{0:n0}] in {1:n0}ms    ValueString[{2:n0}]{3:n0}    Complex[{4:n0}]{5:n0}".SeeArgs(a1.Length, e1, a2.Length,  e2, a3.Length, e3);
    }

    [Run, Aver.Throws(typeof(AzosException), Message = "Length > 24")]
    public void TooLong()
    {
      var v = new VALS("jpofiaojdfopisjapofretertke;wlrkt';lke;rlkt';lewkr;'ltk';lewkr;'tewrkijapsojidfpoisajpofjaspojpiaofsdjf");
    }

    [Run]
    public void Null()
    {
      var vs = new VALS(null);
      Aver.IsNull(vs.StringValue);
      Aver.AreEqual(0, vs.Length);
      Aver.AreEqual(0, vs.GetHashCode());
    }

    [Run]
    public void Blank()
    {
      var vs = new VALS("    ");
      Aver.IsNotNull(vs.StringValue);
      Aver.AreEqual("    ", vs.StringValue);
      Aver.AreEqual(4, vs.Length);
    }

    [Run]
    public void BasicEquality()
    {
      var v1 = new VALS("abcd");
      var v2 = new VALS("abcd");

      Aver.IsTrue(v1.Equals(v2));
      Aver.AreEqual(v1, v2);
      Aver.IsTrue(v1 == v2);
      Aver.IsFalse(v1 != v2);
    }

    [Run]
    public void NullEquality()
    {
      var v1 = new VALS(null);
      var v2 = new VALS(null);

      Aver.IsTrue(v1.Equals(v2));
      Aver.AreEqual(v1, v2);
      Aver.IsTrue(v1 == v2);
      Aver.IsFalse(v1 != v2);
    }

    [Run]
    public void BasicInEquality()
    {
      var v1 = new VALS("abcda");
      var v2 = new VALS("abcd");

      Aver.IsFalse(v1.Equals(v2));
      Aver.AreNotEqual(v1, v2);
      Aver.IsFalse(v1 == v2);
      Aver.IsTrue(v1 != v2);

      v1 = new VALS("abcda");
      v2 = new VALS("abcdA");

      Aver.IsFalse(v1.Equals(v2));
      Aver.AreNotEqual(v1, v2);
      Aver.IsFalse(v1 == v2);
      Aver.IsTrue(v1 != v2);

      v1 = new VALS(null);
      v2 = new VALS(" ");

      Aver.IsFalse(v1.Equals(v2));
      Aver.AreNotEqual(v1, v2);
      Aver.IsFalse(v1 == v2);
      Aver.IsTrue(v1 != v2);

      v1 = new VALS(" ");
      v2 = new VALS(null);

      Aver.IsFalse(v1.Equals(v2));
      Aver.AreNotEqual(v1, v2);
      Aver.IsFalse(v1 == v2);
      Aver.IsTrue(v1 != v2);
    }

    [Run]
    public void BoxingEquality()
    {
      var v1 = new VALS("abcd");
      var v2 = new VALS("abcd");

      object o1 = v1; //boxing
      Aver.AreObjectsEqual(o1, v2);
    }

    [Run]
    public void BoxingInEquality()
    {
      var v1 = new VALS("abcd");
      var v2 = new VALS("abdswfscd");

      object o1 = v1; //boxing
      Aver.AreObjectsNotEqual(o1, v2);
    }

    [Run]
    public void StringEquality()
    {
      var v1 = new VALS("abcd");
      var v2 = "abcd";

      Aver.IsTrue(v2 == v1);
      Aver.IsTrue(v1 == v2);
      Aver.IsTrue(v1.Equals(v2));

      v1 = new VALS(null);
      v2 = null;

      Aver.IsTrue(v2 == v1);
      Aver.IsTrue(v1 == v2);
      Aver.IsTrue(v1.Equals(v2));
    }

    [Run]
    public void StringInEquality()
    {
      var v1 = new VALS("abcqwerd");
      var v2 = "abcd";

      Aver.IsFalse(v2 == v1);
      Aver.IsFalse(v1 == v2);
      Aver.IsFalse(v1.Equals(v2));

      v1 = new VALS("abcqwerd");
      v2 = null;

      Aver.IsFalse(v2 == v1);
      Aver.IsFalse(v1 == v2);
      Aver.IsFalse(v1.Equals(v2));
    }

    [Run]
    public void ToString_Length()
    {
      var v = new VALS("0123456789012345");
      Aver.AreEqual(16, v.Length);
      Aver.AreEqual("0123456789012345", v.ToString());
    }

    [Run]
    public void ToString_Length_Null()
    {
      var v = new VALS(null);
      Aver.AreEqual(0, v.Length);
      Aver.AreEqual((string)null, v.ToString());
    }

    [Run]
    public void ToString_Length_Empty()
    {
      var v = new VALS("");
      Aver.AreEqual(0, v.Length);
      Aver.AreEqual((string)null, v.ToString());
    }

    [Run("  s1='' s2=''  eq=true ")]
    [Run("  s1='a' s2='a' eq=true")]
    [Run("  s1='abcd' s2='abcd' eq=true")]
    [Run("  s1='abcd' s2='abcd' eq=true")]

    [Run("  s1='abcd2' s2='abcd' eq=false")]
    [Run("  s1='abcd2' s2='abcd23' eq=false")]
    [Run("  s1='' s2='' eq=true")]
    [Run("  s1=' ' s2='' eq=false")]
    [Run("  s1='' s2=' ' eq=false")]
    [Run("  s1='1234' s2='1234' eq=true")]
    [Run("  s1='12345' s2='51234' eq=false")]
    [Run("  s1='ABBA' s2='ABBA' eq=true")]
    [Run("  s1='Abba' s2='AbbA' eq=false")]
    public void GetHashCode_Equals(string s1, string s2, bool eq)
    {
      var v1 = new VALS(s1);
      var v2 = new VALS(s2);

      Aver.IsTrue(eq ^ !( v1.GetHashCode() == v2.GetHashCode()));
      Aver.IsTrue(eq ^ !(v1 == v2));
    }

  }
}
