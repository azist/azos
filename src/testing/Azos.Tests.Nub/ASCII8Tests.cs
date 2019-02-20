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
  public class ASCII8Tests
  {
    [Run]
    public void Zero()
    {
      var x = new ASCII8(0);
      Aver.IsTrue( x.IsZero );
      Aver.AreEqual(0ul, x.ID);
    }

    [Run]
    public void Zero_ToString_Value()
    {
      var x = new ASCII8(0);
      Console.WriteLine(x.ToString());
      Aver.AreEqual("ASC8(zero)", x.ToString());
      Aver.IsNull( x.Value );
    }

    [Run]
    public void Zero_Equality()
    {
      var x = new ASCII8(0);
      var y = new ASCII8(0);
      Aver.AreEqual(x, y);
      Aver.IsTrue(x==y);
    }

    [Run]
    public void Zero_InEquality()
    {
      var x = new ASCII8(0);
      var y = new ASCII8(1);
      Aver.AreNotEqual(x, y);
      Aver.IsTrue(x != y);
    }

    [Run]
    public void Test_ToString()
    {
      var x = new ASCII8(0x3041304130413041ul);
      Aver.AreEqual("ASC8(0x3041304130413041, `A0A0A0A0`)", x.ToString());
    }

    [Run]
    public void Encode_ToString()
    {
      var x = ASCII8.Encode("ALEX");
      Aver.AreEqual("ASC8(0x58454C41, `ALEX`)", x.ToString());
    }

    [Run]
    public void Encode_Decode()
    {
      var x = ASCII8.Encode("ALEX1234");
      var y = new ASCII8(x.ID);
      Aver.AreEqual(x, y);
      Aver.AreEqual("ALEX1234", y.Value);
    }

    [Run]
    public void Hashcodes()
    {
      var x = ASCII8.Encode("ALEX1234");
      var y = new ASCII8(x.ID);
      Aver.AreEqual(x, y);
      Aver.AreEqual(x.GetHashCode(), y.GetHashCode());
      Aver.AreEqual(x.GetDistributedStableHash(), y.GetDistributedStableHash());
      Aver.AreEqual("ALEX1234", y.Value);
    }

    [Run]
    public void Encode_Null()
    {
      var x = ASCII8.Encode(null);
      Aver.AreEqual(0ul, x.ID);
      Aver.IsTrue(x.IsZero);
    }

    [Run, Aver.Throws(typeof(AzosException), Message = "blank")]
    public void Error_Empty()
    {
       var x = ASCII8.Encode("");
    }

    [Run, Aver.Throws(typeof(AzosException), Message = "1 and 8")]
    public void Error_ToLong()
    {
      var x = ASCII8.Encode("123456789");
    }

    [Run, Aver.Throws(typeof(AzosException), Message = "!ASCII")]
    public void Error_NonAscii()
    {
      var x = ASCII8.Encode("ag²■");
    }

    [Run]
    public void VarLength()
    {
      var x = ASCII8.Encode("a");
      Console.WriteLine(x.ToString());
      var y = ASCII8.Encode("ab");
      Console.WriteLine(y.ToString());

      Aver.AreEqual(0x61ul, x.ID);
      Aver.AreEqual(0x6261ul, y.ID);
    }

  }
}
