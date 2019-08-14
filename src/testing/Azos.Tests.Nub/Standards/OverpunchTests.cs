/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;
using Azos.Standards;

namespace Azos.Tests.Nub.Standards
{
  [Runnable]
  public class OverpunchTests
  {
    [Run(" v=-1021   o='102J'  ")]
    [Run(" v=-1021   o='102j'  ")]
    [Run(" v=1021    o='102A'  ")]
    [Run(" v=1021    o='102a'  ")]
    [Run(" v=1020    o='102{'  ")]
    [Run(" v=-1020   o='102}'  ")]
    [Run(" v=-100    o='10}'  ")]
    [Run(" v=451     o='45A'  ")]

    [Run(" v=341       o='0000000000034a'  ")]
    [Run(" v=-87543    o='00000008754l'  ")]
    [Run(" v=8298380   o='00000829838{'  ")]
    [Run(" v=-437240   o='00000043724}'  ")]
    public void Basic(long v, string o)
    {
      Aver.AreEqual(v, Overpunch.ToLong(o));
      Aver.IsTrue( o.EqualsOrdIgnoreCase( Overpunch.FromLong(v)));
    }

    [Run]
    public void Long_DefaultNull()
    {
      Aver.AreEqual(null, Overpunch.ToLong(null));
    }

    [Run]
    public void Long_DefaultOnError()
    {
      Aver.AreEqual(-5, Overpunch.ToLong("zgyug", -5, Data.ConvertErrorHandling.ReturnDefault));
    }

    [Run]
    [Aver.Throws(typeof(AzosException), "not parsable")]
    public void Long_ThrowOnError()
    {
      Overpunch.ToLong("zgyug", -5, Data.ConvertErrorHandling.Throw);
    }



    [Run(" v=-1021   o='102J'  ")]
    [Run(" v=-1021   o='102j'  ")]
    [Run(" v=1021    o='102A'  ")]
    [Run(" v=1021    o='102a'  ")]
    [Run(" v=1020    o='102{'  ")]
    [Run(" v=-1020   o='102}'  ")]
    [Run(" v=-100    o='10}'  ")]
    [Run(" v=451     o='45A'  ")]

    [Run(" v=341       o='0000000000034a'  ")]
    [Run(" v=-87543    o='00000008754l'  ")]
    [Run(" v=8298380   o='00000829838{'  ")]
    [Run(" v=-437240   o='00000043724}'  ")]
    public void Decimal_Basic(decimal v, string o) => Aver.AreEqual(v, Overpunch.ToDecimal(o, 1));

    [Run(" v=-102.1  s=10  o='102J'  ")]
    [Run(" v=-102.1  s=10  o='102j'  ")]
    [Run(" v=102.1   s=10 o='102A'  ")]
    [Run(" v=102.1   s=10 o='102a'  ")]
    [Run(" v=102.0   s=10 o='102{'  ")]
    [Run(" v=-102.0  s=10 o='102}'  ")]
    [Run(" v=-10.0   s=10 o='10}'  ")]
    [Run(" v=45.1    s=10 o='45A'  ")]
    [Run(" v=34.1      s=10 o='0000000000034a'  ")]
    [Run(" v=-8754.3   s=10 o='00000008754l'  ")]
    [Run(" v=829838.0  s=10 o='00000829838{'  ")]
    [Run(" v=-43724.0  s=10 o='00000043724}'  ")]

    [Run(" v=-10.21  s=100  o='102J'  ")]
    [Run(" v=-10.21  s=100  o='102j'  ")]
    public void Decimal_Scaled(decimal v, int s, string o) => Aver.AreEqual(v, Overpunch.ToDecimal(o, s));

    [Run]
    public void Decimal_DefaultNull()
    {
      Aver.AreEqual(null, Overpunch.ToDecimal(null));
    }

    [Run]
    public void Decimal_DefaultOnError()
    {
      Aver.AreEqual(-5.17M, Overpunch.ToDecimal("zgyug", dflt: -5.17M, handling: Data.ConvertErrorHandling.ReturnDefault));
    }

    [Run]
    public void Decimal_DefaultOnError_Scaled()
    {
      Aver.AreEqual(-5.7898M, Overpunch.ToDecimal("zgyug", 1_0000, -5.7898M, Data.ConvertErrorHandling.ReturnDefault));
    }

    [Run]
    [Aver.Throws(typeof(AzosException), "not parsable")]
    public void Decimal_ThrowOnError()
    {
      Overpunch.ToDecimal("zgyug", 10,  -5.23M, Data.ConvertErrorHandling.Throw);
    }

  }
}
