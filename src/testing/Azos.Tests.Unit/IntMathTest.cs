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

namespace Azos.Tests.Unit
{
    [Runnable(TRUN.BASE)]
    public class IntMathTest
    {
        [Run]
        public void IntMathPower()
        {
            Aver.AreEqual(1024,   IntMath.Pow(2, 10));
            Aver.AreEqual(2187,   IntMath.Pow(3, 7));
            Aver.AreEqual(390625, IntMath.Pow(5, 8));
            Aver.AreEqual(0,      IntMath.Pow(0, 0));
            Aver.AreEqual(0,      IntMath.Pow(0, 1));
            Aver.AreEqual(1,      IntMath.Pow(1, 0));
            Aver.AreEqual(1,      IntMath.Pow(100, 0));
            Aver.AreEqual(100,    IntMath.Pow(100, 1));
        }

        [Run]
        public void IntMathLog()
        {
            Aver.AreEqual(10, IntMath.Log(1024, 2));
            Aver.AreEqual(2, IntMath.Log(9, 3));
            Aver.AreEqual(2, IntMath.Log(11, 3));
            Aver.AreEqual(1, IntMath.Log(2, 2));
            Aver.AreEqual(0, IntMath.Log(1, 2));
            Aver.Throws<AzosException>( () => IntMath.Log(0, 2) );

            Aver.AreEqual(62, IntMath.Log(1L << 62, 2));
            Aver.AreEqual(32, IntMath.Log(1L << 32, 2));
            Aver.AreEqual(10, IntMath.Log(1024, 2));
            Aver.AreEqual(4,  IntMath.Log(16, 2));
            Aver.AreEqual(3,  IntMath.Log(8, 2));
            Aver.AreEqual(1,  IntMath.Log(2, 2));
            Aver.AreEqual(0,  IntMath.Log(1, 2));
        }

        [Run]
        public void IntMathUpperPower()
        {
            Aver.AreEqual(1024,   IntMath.UpperPow(1024, 2));
            Aver.AreEqual(9,      IntMath.UpperPow(9, 3));
            Aver.AreEqual(27,     IntMath.UpperPow(11, 3));
            Aver.AreEqual(16,     IntMath.UpperPow(14, 2));
            Aver.AreEqual(16,     IntMath.UpperPow(15, 2));
            Aver.AreEqual(16,     IntMath.UpperPow(16, 2));
            Aver.AreEqual(4,      IntMath.UpperPow(3, 2));
        }

        [Run]
        public void MinMaxTest()
        {
            Aver.AreEqual(1,  IntMath.MinMax(-1, 1, 3));
            Aver.AreEqual(2,  IntMath.MinMax(2, 2, 3));
            Aver.AreEqual(3,  IntMath.MinMax(2, 3, 3));
            Aver.AreEqual(-1, IntMath.MinMax(-1, -10, 3));
            Aver.AreEqual(3,  IntMath.MinMax(-1, 5, 3));
            Aver.Throws<AzosException>(() => IntMath.MinMax(10, 5, 3));
        }


        [Run]
        public void Align8_int()
        {
            Aver.AreEqual(8,    IntMath.Align8(3));
            Aver.AreEqual(40,   IntMath.Align8(33));
            Aver.AreEqual(0,    IntMath.Align8(0));
            Aver.AreEqual(8,    IntMath.Align8(8));
            Aver.AreEqual(16,   IntMath.Align8(16));
            Aver.AreEqual(24,   IntMath.Align8(17));
            Aver.AreEqual(128,  IntMath.Align8(127));
            Aver.AreEqual(128,  IntMath.Align8(128));
            Aver.AreEqual(136,  IntMath.Align8(129));

            Aver.AreEqual(0,   IntMath.Align8(-5));
            Aver.AreEqual(-8,  IntMath.Align8(-10));
        }

        [Run]
        public void Align8_long()
        {
            Aver.AreEqual(8,    IntMath.Align8(3L));
            Aver.AreEqual(40,   IntMath.Align8(33L));
            Aver.AreEqual(0,    IntMath.Align8(0L));
            Aver.AreEqual(8,    IntMath.Align8(8L));
            Aver.AreEqual(16,   IntMath.Align8(16L));
            Aver.AreEqual(24,   IntMath.Align8(17L));
            Aver.AreEqual(128,  IntMath.Align8(127L));
            Aver.AreEqual(128,  IntMath.Align8(128L));
            Aver.AreEqual(136,  IntMath.Align8(129L));

            Aver.AreEqual(0,   IntMath.Align8(-5L));
            Aver.AreEqual(-8,  IntMath.Align8(-10L));
        }

        [Run]
        public void Align16_int()
        {
            Aver.AreEqual(16,    IntMath.Align16(3));
            Aver.AreEqual(48,    IntMath.Align16(33));
            Aver.AreEqual(0,     IntMath.Align16(0));
            Aver.AreEqual(16,    IntMath.Align16(8));
            Aver.AreEqual(16,    IntMath.Align16(16));
            Aver.AreEqual(32,    IntMath.Align16(17));
            Aver.AreEqual(128,   IntMath.Align16(127));
            Aver.AreEqual(128,   IntMath.Align16(128));
            Aver.AreEqual(144,   IntMath.Align16(129));

            Aver.AreEqual(0,     IntMath.Align16(-5));
            Aver.AreEqual(-16,   IntMath.Align16(-17));
        }

        [Run]
        public void Align16_long()
        {
            Aver.AreEqual(16,    IntMath.Align16(3L));
            Aver.AreEqual(48,    IntMath.Align16(33L));
            Aver.AreEqual(0,     IntMath.Align16(0L));
            Aver.AreEqual(16,    IntMath.Align16(8L));
            Aver.AreEqual(16,    IntMath.Align16(16L));
            Aver.AreEqual(32,    IntMath.Align16(17L));
            Aver.AreEqual(128,   IntMath.Align16(127L));
            Aver.AreEqual(128,   IntMath.Align16(128L));
            Aver.AreEqual(144,   IntMath.Align16(129L));

            Aver.AreEqual(0,    IntMath.Align16(-5L));
            Aver.AreEqual(-16,  IntMath.Align16(-17L));
        }

        [Run]
        public void IsPrime()
        {
            Aver.IsTrue( IntMath.IsPrime( 2 ) );
            Aver.IsTrue( IntMath.IsPrime( 3 ) );
            Aver.IsTrue( IntMath.IsPrime( 7 ) );
            Aver.IsTrue( IntMath.IsPrime( 239 ) );
            Aver.IsTrue( IntMath.IsPrime( 62851 ) );
            Aver.IsTrue( IntMath.IsPrime( 7199369 ) );

            Aver.IsFalse( IntMath.IsPrime( -1 ) );
            Aver.IsFalse( IntMath.IsPrime( 0 ) );
            Aver.IsFalse( IntMath.IsPrime( 1 ) );
            Aver.IsFalse( IntMath.IsPrime( 4 ) );
            Aver.IsFalse( IntMath.IsPrime( 6 ) );
            Aver.IsFalse( IntMath.IsPrime( 8 ) );
            Aver.IsFalse( IntMath.IsPrime( 9 ) );
            Aver.IsFalse( IntMath.IsPrime( 10 ) );
            Aver.IsFalse( IntMath.IsPrime( 20 ) );
            Aver.IsFalse( IntMath.IsPrime( 120 ) );
            Aver.IsFalse( IntMath.IsPrime( 1000 ) );

        }

        [Run]
        public void GetAdjacentPrimeNumberLessThan_1()
        {
           Aver.AreEqual(2, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(-10));
           Aver.AreEqual(2, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(0));
           Aver.AreEqual(2, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(1));
           Aver.AreEqual(2, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(2));
           Aver.AreEqual(3, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(3));

           Aver.AreEqual(3, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(4));
           Aver.AreEqual(5, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(5));
           Aver.AreEqual(5, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(6));
           Aver.AreEqual(7, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(7));

           Aver.AreEqual(107, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(107));
           Aver.AreEqual(107, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(108));
           Aver.AreEqual(109, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(109));
           Aver.AreEqual(109, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(110));
           Aver.AreEqual(109, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(111));
           Aver.AreEqual(109, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(112));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(113));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(114));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(115));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(116));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(117));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(118));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(119));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(120));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(121));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(122));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(123));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(124));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(125));
           Aver.AreEqual(113, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(126));
           Aver.AreEqual(127, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(127));


           Aver.AreEqual(631, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(633));

           Aver.AreEqual(2459, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(2465));

           Aver.AreEqual(1148747, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(1148747));
           Aver.AreEqual(1148747, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(1148748));
           Aver.AreEqual(1148747, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(1148752));
           Aver.AreEqual(1148753, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(1148753));

           Aver.AreEqual(15485857, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(15485862));
           Aver.AreEqual(15485863, IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(15485863));
        }

        [Run]
        public void GetAdjacentPrimeNumberLessThan_2()
        {
          for(var i=2; i<1000000; i++)
           Aver.IsTrue( IntMath.IsPrime( IntMath.GetAdjacentPrimeNumberLessThanOrEqualTo(i) ) );
        }

         [Run]
        public void PRIME_CAPACITIES()
        {
          foreach(var capacity in IntMath.PRIME_CAPACITIES)
           Aver.IsTrue( IntMath.IsPrime( capacity ) );
        }

        [Run]
        public void GetPrimeCapacityOfAtLeast_1()
        {
          Aver.AreEqual(7  , IntMath.GetPrimeCapacityOfAtLeast( 4 ));
          Aver.AreEqual(59  , IntMath.GetPrimeCapacityOfAtLeast( 48 ));
          Aver.AreEqual(71  , IntMath.GetPrimeCapacityOfAtLeast( 64 ));

           Aver.AreEqual(131  , IntMath.GetPrimeCapacityOfAtLeast( 128 ));
           Aver.AreEqual(131  , IntMath.GetPrimeCapacityOfAtLeast( 129 ));
           Aver.AreEqual(131  , IntMath.GetPrimeCapacityOfAtLeast( 130 ));
           Aver.AreEqual(131  , IntMath.GetPrimeCapacityOfAtLeast( 131 ));
           Aver.AreEqual(163  , IntMath.GetPrimeCapacityOfAtLeast( 132 ));

          Aver.AreEqual(672827  , IntMath.GetPrimeCapacityOfAtLeast( 672800 ));

          Aver.AreEqual(334231259  , IntMath.GetPrimeCapacityOfAtLeast( 334231259  ));
          Aver.AreEqual(334231291  , IntMath.GetPrimeCapacityOfAtLeast( 334231260  ));
        }

        [Run]
        public void GetPrimeCapacityOfAtLeast_2()
        {
          for(var i=2; i<1000000; i++)
          {
           var cap = IntMath.GetPrimeCapacityOfAtLeast(i);
           Aver.IsTrue( cap >= i);
           Aver.IsTrue( cap < i*2);
           Aver.IsTrue( IntMath.IsPrime( cap ) );
          }
        }

        [Run]
        public void GetCapacityFactoredToPrime_1()
        {
          Aver.AreEqual(11  , IntMath.GetCapacityFactoredToPrime( 4, 2d ) );
          Aver.AreEqual(37  , IntMath.GetCapacityFactoredToPrime( 16, 2d ) );
          Aver.AreEqual(59  , IntMath.GetCapacityFactoredToPrime( 16, 3d ) );

          Aver.AreEqual(521  , IntMath.GetCapacityFactoredToPrime( 256, 2d ) );

          Aver.AreEqual(2333  , IntMath.GetCapacityFactoredToPrime( 1024, 2d ) );

          Aver.AreEqual(521  , IntMath.GetCapacityFactoredToPrime( 1024, 0.5d ) );
          Aver.AreEqual(293  , IntMath.GetCapacityFactoredToPrime( 1024, 0.25d ) );

          Aver.AreEqual(2411033  , IntMath.GetCapacityFactoredToPrime( 1024 * 1024, 2d) );
          Aver.AreEqual(16777259  , IntMath.GetCapacityFactoredToPrime( 8 * 1024 * 1024, 2d) );
          Aver.AreEqual(33554467  , IntMath.GetCapacityFactoredToPrime( 16 * 1024 * 1024, 2d) );
        }

        [Run]
        public void GetCapacityFactoredToPrime_2()
        {
          int cap = 16;
          while(cap<2000000000)
          {
            cap = IntMath.GetCapacityFactoredToPrime(cap, 2d);
            Console.WriteLine(cap);
            Aver.IsTrue( IntMath.IsPrime( cap ) );
          }
        }
    }
}
