/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Collections;
using Azos.Scripting;

namespace Azos.Tests.Nub.BaseCollections
{
  [Runnable]
  public class BitListTest
  {
    [Run]
    [Aver.Throws(typeof(ArgumentOutOfRangeException))]
    public void OutOfRange()
    {
      BitList bits = new BitList();
      var b = bits[0];
    }

    [Run]
    public void AppendBit()
    {
      BitList bits = new BitList();

      Aver.AreEqual(0, bits.Size);

      bits.AppendBit(true);

      Aver.AreEqual(1, bits.Size);
      Aver.AreEqual(true, bits[0]);

      bits.AppendBit(false);
      bits.AppendBit(true);
      Aver.AreEqual(3, bits.Size);
      Aver.AreEqual(false, bits[1]);
      Aver.AreEqual(true, bits[2]);
    }

    [Run]
    public void AppendOther()
    {
      const int SIZE0 = 13, SIZE1 = 33;

      BitList bits0 = new BitList();
      for (int i = 0; i < SIZE0; i++)
        bits0.AppendBit(true);

      BitList bits1 = new BitList();
      for (int i = 0; i < SIZE1; i++)
        bits1.AppendBit(true);

      bits0.AppendBitList(bits1);

      Aver.AreEqual( SIZE0 + SIZE1, bits0.Size);
    }

  }
}
