/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class IntMathTests
  {
    [Run]
    public void IntMathPower()
    {
      Aver.AreEqual(1024, IntUtils.Pow(2, 10));
      Aver.AreEqual(2187, IntUtils.Pow(3, 7));
      Aver.AreEqual(390625, IntUtils.Pow(5, 8));
      Aver.AreEqual(0, IntUtils.Pow(0, 0));
      Aver.AreEqual(0, IntUtils.Pow(0, 1));
      Aver.AreEqual(1, IntUtils.Pow(1, 0));
      Aver.AreEqual(1, IntUtils.Pow(100, 0));
      Aver.AreEqual(100, IntUtils.Pow(100, 1));
    }

    [Run]
    public void IntMathLog()
    {
      Aver.AreEqual(10, IntUtils.Log(1024, 2));
      Aver.AreEqual(2, IntUtils.Log(9, 3));
      Aver.AreEqual(2, IntUtils.Log(11, 3));
      Aver.AreEqual(1, IntUtils.Log(2, 2));
      Aver.AreEqual(0, IntUtils.Log(1, 2));
      Aver.Throws<AzosException>(() => IntUtils.Log(0, 2));

      Aver.AreEqual(62, IntUtils.Log(1L << 62, 2));
      Aver.AreEqual(32, IntUtils.Log(1L << 32, 2));
      Aver.AreEqual(10, IntUtils.Log(1024, 2));
      Aver.AreEqual(4, IntUtils.Log(16, 2));
      Aver.AreEqual(3, IntUtils.Log(8, 2));
      Aver.AreEqual(1, IntUtils.Log(2, 2));
      Aver.AreEqual(0, IntUtils.Log(1, 2));
    }

    [Run]
    public void IntMathUpperPower()
    {
      Aver.AreEqual(1024, IntUtils.UpperPow(1024, 2));
      Aver.AreEqual(9, IntUtils.UpperPow(9, 3));
      Aver.AreEqual(27, IntUtils.UpperPow(11, 3));
      Aver.AreEqual(16, IntUtils.UpperPow(14, 2));
      Aver.AreEqual(16, IntUtils.UpperPow(15, 2));
      Aver.AreEqual(16, IntUtils.UpperPow(16, 2));
      Aver.AreEqual(4, IntUtils.UpperPow(3, 2));
    }

    [Run]
    public void MinMaxTest()
    {
      Aver.AreEqual(1, IntUtils.MinMax(-1, 1, 3));
      Aver.AreEqual(2, IntUtils.MinMax(2, 2, 3));
      Aver.AreEqual(3, IntUtils.MinMax(2, 3, 3));
      Aver.AreEqual(-1, IntUtils.MinMax(-1, -10, 3));
      Aver.AreEqual(3, IntUtils.MinMax(-1, 5, 3));
      Aver.Throws<AzosException>(() => IntUtils.MinMax(10, 5, 3));
    }

    [Run]
    public void Align8_int()
    {
      Aver.AreEqual(8, IntUtils.Align8(3));
      Aver.AreEqual(40, IntUtils.Align8(33));
      Aver.AreEqual(0, IntUtils.Align8(0));
      Aver.AreEqual(8, IntUtils.Align8(8));
      Aver.AreEqual(16, IntUtils.Align8(16));
      Aver.AreEqual(24, IntUtils.Align8(17));
      Aver.AreEqual(128, IntUtils.Align8(127));
      Aver.AreEqual(128, IntUtils.Align8(128));
      Aver.AreEqual(136, IntUtils.Align8(129));

      Aver.AreEqual(0, IntUtils.Align8(-5));
      Aver.AreEqual(-8, IntUtils.Align8(-10));
    }

    [Run]
    public void Align8_long()
    {
      Aver.AreEqual(8, IntUtils.Align8(3L));
      Aver.AreEqual(40, IntUtils.Align8(33L));
      Aver.AreEqual(0, IntUtils.Align8(0L));
      Aver.AreEqual(8, IntUtils.Align8(8L));
      Aver.AreEqual(16, IntUtils.Align8(16L));
      Aver.AreEqual(24, IntUtils.Align8(17L));
      Aver.AreEqual(128, IntUtils.Align8(127L));
      Aver.AreEqual(128, IntUtils.Align8(128L));
      Aver.AreEqual(136, IntUtils.Align8(129L));

      Aver.AreEqual(0, IntUtils.Align8(-5L));
      Aver.AreEqual(-8, IntUtils.Align8(-10L));
    }

    [Run]
    public void Align16_int()
    {
      Aver.AreEqual(16, IntUtils.Align16(3));
      Aver.AreEqual(48, IntUtils.Align16(33));
      Aver.AreEqual(0, IntUtils.Align16(0));
      Aver.AreEqual(16, IntUtils.Align16(8));
      Aver.AreEqual(16, IntUtils.Align16(16));
      Aver.AreEqual(32, IntUtils.Align16(17));
      Aver.AreEqual(128, IntUtils.Align16(127));
      Aver.AreEqual(128, IntUtils.Align16(128));
      Aver.AreEqual(144, IntUtils.Align16(129));

      Aver.AreEqual(0, IntUtils.Align16(-5));
      Aver.AreEqual(-16, IntUtils.Align16(-17));
    }

    [Run]
    public void Align16_long()
    {
      Aver.AreEqual(16, IntUtils.Align16(3L));
      Aver.AreEqual(48, IntUtils.Align16(33L));
      Aver.AreEqual(0, IntUtils.Align16(0L));
      Aver.AreEqual(16, IntUtils.Align16(8L));
      Aver.AreEqual(16, IntUtils.Align16(16L));
      Aver.AreEqual(32, IntUtils.Align16(17L));
      Aver.AreEqual(128, IntUtils.Align16(127L));
      Aver.AreEqual(128, IntUtils.Align16(128L));
      Aver.AreEqual(144, IntUtils.Align16(129L));

      Aver.AreEqual(0, IntUtils.Align16(-5L));
      Aver.AreEqual(-16, IntUtils.Align16(-17L));
    }

    [Run]
    public void IsPrime()
    {
      Aver.IsTrue(IntUtils.IsPrime(2));
      Aver.IsTrue(IntUtils.IsPrime(3));
      Aver.IsTrue(IntUtils.IsPrime(7));
      Aver.IsTrue(IntUtils.IsPrime(239));
      Aver.IsTrue(IntUtils.IsPrime(62851));
      Aver.IsTrue(IntUtils.IsPrime(7199369));

      Aver.IsFalse(IntUtils.IsPrime(-1));
      Aver.IsFalse(IntUtils.IsPrime(0));
      Aver.IsFalse(IntUtils.IsPrime(1));
      Aver.IsFalse(IntUtils.IsPrime(4));
      Aver.IsFalse(IntUtils.IsPrime(6));
      Aver.IsFalse(IntUtils.IsPrime(8));
      Aver.IsFalse(IntUtils.IsPrime(9));
      Aver.IsFalse(IntUtils.IsPrime(10));
      Aver.IsFalse(IntUtils.IsPrime(20));
      Aver.IsFalse(IntUtils.IsPrime(120));
      Aver.IsFalse(IntUtils.IsPrime(1000));
    }

    [Run]
    public void GetAdjacentPrimeNumberLessThan_1()
    {
      Aver.AreEqual(2, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(-10));
      Aver.AreEqual(2, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(0));
      Aver.AreEqual(2, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(1));
      Aver.AreEqual(2, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(2));
      Aver.AreEqual(3, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(3));

      Aver.AreEqual(3, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(4));
      Aver.AreEqual(5, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(5));
      Aver.AreEqual(5, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(6));
      Aver.AreEqual(7, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(7));

      Aver.AreEqual(107, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(107));
      Aver.AreEqual(107, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(108));
      Aver.AreEqual(109, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(109));
      Aver.AreEqual(109, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(110));
      Aver.AreEqual(109, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(111));
      Aver.AreEqual(109, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(112));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(113));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(114));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(115));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(116));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(117));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(118));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(119));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(120));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(121));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(122));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(123));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(124));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(125));
      Aver.AreEqual(113, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(126));
      Aver.AreEqual(127, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(127));


      Aver.AreEqual(631, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(633));

      Aver.AreEqual(2459, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(2465));

      Aver.AreEqual(1148747, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(1148747));
      Aver.AreEqual(1148747, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(1148748));
      Aver.AreEqual(1148747, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(1148752));
      Aver.AreEqual(1148753, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(1148753));

      Aver.AreEqual(15485857, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(15485862));
      Aver.AreEqual(15485863, IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(15485863));
    }

    [Run]
    public void GetAdjacentPrimeNumberLessThan_2()
    {
      for (var i = 2; i < 1000000; i++)
        Aver.IsTrue(IntUtils.IsPrime(IntUtils.GetAdjacentPrimeNumberLessThanOrEqualTo(i)));
    }

    [Run]
    public void PRIME_CAPACITIES()
    {
      foreach (var capacity in IntUtils.PRIME_CAPACITIES)
        Aver.IsTrue(IntUtils.IsPrime(capacity));
    }

    [Run]
    public void GetPrimeCapacityOfAtLeast_1()
    {
      Aver.AreEqual(7, IntUtils.GetPrimeCapacityOfAtLeast(4));
      Aver.AreEqual(59, IntUtils.GetPrimeCapacityOfAtLeast(48));
      Aver.AreEqual(71, IntUtils.GetPrimeCapacityOfAtLeast(64));

      Aver.AreEqual(131, IntUtils.GetPrimeCapacityOfAtLeast(128));
      Aver.AreEqual(131, IntUtils.GetPrimeCapacityOfAtLeast(129));
      Aver.AreEqual(131, IntUtils.GetPrimeCapacityOfAtLeast(130));
      Aver.AreEqual(131, IntUtils.GetPrimeCapacityOfAtLeast(131));
      Aver.AreEqual(163, IntUtils.GetPrimeCapacityOfAtLeast(132));

      Aver.AreEqual(672827, IntUtils.GetPrimeCapacityOfAtLeast(672800));

      Aver.AreEqual(334231259, IntUtils.GetPrimeCapacityOfAtLeast(334231259));
      Aver.AreEqual(334231291, IntUtils.GetPrimeCapacityOfAtLeast(334231260));
    }

    [Run]
    public void GetPrimeCapacityOfAtLeast_2()
    {
      for (var i = 2; i < 1000000; i++)
      {
        var cap = IntUtils.GetPrimeCapacityOfAtLeast(i);
        Aver.IsTrue(cap >= i);
        Aver.IsTrue(cap < i * 2);
        Aver.IsTrue(IntUtils.IsPrime(cap));
      }
    }

    [Run]
    public void GetCapacityFactoredToPrime_1()
    {
      Aver.AreEqual(11, IntUtils.GetCapacityFactoredToPrime(4, 2d));
      Aver.AreEqual(37, IntUtils.GetCapacityFactoredToPrime(16, 2d));
      Aver.AreEqual(59, IntUtils.GetCapacityFactoredToPrime(16, 3d));

      Aver.AreEqual(521, IntUtils.GetCapacityFactoredToPrime(256, 2d));

      Aver.AreEqual(2333, IntUtils.GetCapacityFactoredToPrime(1024, 2d));

      Aver.AreEqual(521, IntUtils.GetCapacityFactoredToPrime(1024, 0.5d));
      Aver.AreEqual(293, IntUtils.GetCapacityFactoredToPrime(1024, 0.25d));

      Aver.AreEqual(2411033, IntUtils.GetCapacityFactoredToPrime(1024 * 1024, 2d));
      Aver.AreEqual(16777259, IntUtils.GetCapacityFactoredToPrime(8 * 1024 * 1024, 2d));
      Aver.AreEqual(33554467, IntUtils.GetCapacityFactoredToPrime(16 * 1024 * 1024, 2d));
    }

    [Run]
    public void GetCapacityFactoredToPrime_2()
    {
      int cap = 16;
      while (cap < 2000000000)
      {
        cap = IntUtils.GetCapacityFactoredToPrime(cap, 2d);
        cap.See();
        Aver.IsTrue(IntUtils.IsPrime(cap));
      }
    }

  }
}
