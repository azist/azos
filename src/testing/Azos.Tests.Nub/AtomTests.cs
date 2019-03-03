/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class AtomTests
  {
    [Run]
    public void Zero()
    {
      var x = new Atom(0);
      Aver.IsTrue( x.IsZero );
      Aver.AreEqual(0ul, x.ID);
      Aver.IsTrue( x.IsValid );
    }

    [Run]
    public void Zero_ToString_Value()
    {
      var x = new Atom(0);
      Console.WriteLine(x.ToString());
      Aver.AreEqual("Atom(zero)", x.ToString());
      Aver.IsNull( x.Value );
    }

    [Run]
    public void Zero_Equality()
    {
      var x = new Atom(0);
      var y = new Atom(0);
      Aver.AreEqual(x, y);
      Aver.IsTrue(x==y);
    }

    [Run]
    public void Zero_InEquality()
    {
      var x = new Atom(0);
      var y = new Atom(1);
      Aver.AreNotEqual(x, y);
      Aver.IsTrue(x != y);
    }

    [Run]
    public void Test_ToString()
    {
      var x = new Atom(0x3041304130413041ul);
      Aver.AreEqual("Atom(0x3041304130413041, `A0A0A0A0`)", x.ToString());
    }

    [Run]
    public void Encode_ToString()
    {
      var x = Atom.Encode("ALEX");
      Aver.AreEqual("Atom(0x58454C41, `ALEX`)", x.ToString());
    }

    [Run]
    public void Encode_Decode()
    {
      var x = Atom.Encode("ALEX1234");
      var y = new Atom(x.ID);
      Aver.AreEqual(x, y);
      Aver.AreEqual("ALEX1234", y.Value);
    }

    [Run]
    public void Hashcodes()
    {
      var x = Atom.Encode("ALEX1234");
      var y = new Atom(x.ID);
      Aver.AreEqual(x, y);
      Aver.AreEqual(x.GetHashCode(), y.GetHashCode());
      Aver.AreEqual(x.GetDistributedStableHash(), y.GetDistributedStableHash());
      Aver.AreEqual("ALEX1234", y.Value);
    }

    [Run]
    public void Encode_Null()
    {
      var x = Atom.Encode(null);
      Aver.AreEqual(0ul, x.ID);
      Aver.IsTrue(x.IsZero);
    }

    [Run, Aver.Throws(typeof(AzosException), Message = "blank")]
    public void Error_Empty()
    {
       var x = Atom.Encode("");
    }

    [Run, Aver.Throws(typeof(AzosException), Message = "1 and 8")]
    public void Error_ToLong()
    {
      var x = Atom.Encode("123456789");
    }

    [Run, Aver.Throws(typeof(AzosException), Message = "![0..9")]
    public void Error_NonAscii()
    {
      var x = Atom.Encode("ag²■");
    }

    [Run]
    public void VarLength()
    {
      var x = Atom.Encode("a");
      Console.WriteLine(x.ToString());
      var y = Atom.Encode("ab");
      Console.WriteLine(y.ToString());

      Aver.AreEqual(0x61ul, x.ID);
      Aver.AreEqual(0x6261ul, y.ID);
    }

    [Run]
    public void IsValid()
    {
      var x = new Atom(0);
      Aver.IsTrue(x.IsZero);
      Aver.IsTrue(x.IsValid);

      x = new Atom(0xffff);
      Aver.IsFalse(x.IsValid);
      Aver.Throws<AzosException>(()=>x.Value.ToLower());
    }

    [Run]
    public void ValueInterning()
    {
      var x = Atom.Encode("abc");
      var y = Atom.Encode("abc");
      var z = new Atom(x.ID);

      Aver.AreEqual(x, y);
      Aver.AreEqual(x, z);

      Aver.AreEqual("abc", x.Value);
      Aver.AreEqual("abc", y.Value);
      Aver.AreEqual("abc", z.Value);

      Aver.AreNotSameRef("abc", x.Value);
      Aver.AreSameRef(x.Value, y.Value);
      Aver.AreSameRef(x.Value, z.Value);
    }
  }
}
