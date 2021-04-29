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
    public void ToLong_Basic(long v, string o)
    {
      Aver.AreEqual(v, Overpunch.ToLong(o));
    }

    [Run(" v=-1021   o='102J'  ")]
    [Run(" v=1021    o='102A'  ")]
    [Run(" v=1022    o='102B'  ")]
    [Run(" v=1020    o='102{'  ")]
    [Run(" v=-1020   o='102}'  ")]
    [Run(" v=-100    o='10}'  ")]
    [Run(" v=451     o='45A'  ")]

    [Run(" v=234341       o='23434A'  ")]
    [Run(" v=-11087543    o='1108754L'  ")]
    [Run(" v=8298380   o='829838{'  ")]
    [Run(" v=-437240   o='43724}'  ")]
    public void FromLong_Basic(long v, string o)
    {
      Aver.AreEqual(o, Overpunch.FromLong(v));
    }

    [Run]
    public void ToLong_DefaultNull()
    {
      Aver.AreEqual(null, Overpunch.ToLong(null));
    }

    [Run]
    public void FromLong_DefaultNull()
    {
      Aver.AreEqual(null, Overpunch.FromLong(null));
      Aver.AreEqual("{", Overpunch.FromLong(0));
    }

    [Run]
    public void FromToLong_Cycle()
    {
      for(var i=-1_000_000; i<=1_000_000; i++)
      {
        var op = Overpunch.FromLong(i);
        var got = Overpunch.ToLong(op);
        Aver.AreEqual(i, got);
      }
    }

    [Run]
    public void ToLong_DefaultOnError()
    {
      Aver.AreEqual(-5, Overpunch.ToLong("zgyug", -5, Data.ConvertErrorHandling.ReturnDefault));
    }

    [Run]
    [Aver.Throws(typeof(AzosException), "not parsable")]
    public void TOLong_ThrowOnError()
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
    public void ToDecimal_Basic(decimal v, string o) => Aver.AreEqual(v, Overpunch.ToDecimal(o, 1));

    [Run(" v=-1021   o='102J'  ")]
    [Run(" v=-41021   o='4102J'  ")]
    [Run(" v=1021    o='102A'  ")]
    [Run(" v=100021    o='10002A'  ")]
    [Run(" v=1020    o='102{'  ")]
    [Run(" v=-1020   o='102}'  ")]
    [Run(" v=-100    o='10}'  ")]
    [Run(" v=451     o='45A'  ")]

    [Run(" v=341       o='34A'  ")]
    [Run(" v=-87543    o='8754L'  ")]
    [Run(" v=8298380   o='829838{'  ")]
    [Run(" v=-437240   o='43724}'  ")]
    public void FromDecimal_Basic(decimal v, string o) => Aver.AreEqual(o, Overpunch.FromDecimal(v, 1));

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
    public void ToDecimal_Scaled(decimal v, int s, string o) => Aver.AreEqual(v, Overpunch.ToDecimal(o, s));

    [Run(" v=-102.1  s=10  o='102J'  ")]
    [Run(" v=-1002.1  s=10  o='1002J'  ")]
    [Run(" v=102.1   s=10 o='102A'  ")]
    [Run(" v=10.21   s=100 o='102A'  ")]
    [Run(" v=102.0   s=10 o='102{'  ")]
    [Run(" v=-102.0  s=10 o='102}'  ")]
    [Run(" v=-10.0   s=10 o='10}'  ")]
    [Run(" v=45.1    s=10 o='45A'  ")]
    [Run(" v=34.1      s=10 o='34A'  ")]
    [Run(" v=-8754.3   s=10 o='8754L'  ")]
    [Run(" v=829838.0  s=10 o='829838{'  ")]
    [Run(" v=-43724.0  s=10 o='43724}'  ")]

    [Run(" v=-10.21  s=100  o='102J'  ")]
    [Run(" v=-1000.021  s=1000  o='100002J'  ")]
    public void FromDecimal_Scaled(decimal v, int s, string o) => Aver.AreEqual(o, Overpunch.FromDecimal(v, s));

    [Run]
    public void ToDecimal_DefaultNull()
    {
      Aver.AreEqual(null, Overpunch.ToDecimal(null));
    }

    [Run]
    public void ToDecimal_DefaultOnError()
    {
      Aver.AreEqual(-5.17M, Overpunch.ToDecimal("zgyug", dflt: -5.17M, handling: Data.ConvertErrorHandling.ReturnDefault));
    }

    [Run]
    public void FromToDecimal_Cycle()
    {
      for(var s = 1000; s >1; s/=10)
        for (var i = -1_000m; i <= 1_000m; i+=0.1m)
        {
          var op = Overpunch.FromDecimal(i, s);
          var got = Overpunch.ToDecimal(op, s);
          Aver.AreEqual(i, got);
        }
    }

    [Run]
    public void FromDecimal_DefaultNull()
    {
      Aver.AreEqual(null, Overpunch.FromDecimal(null));
      Aver.AreEqual("{", Overpunch.FromDecimal(0));
    }

    [Run]
    public void ToDecimal_DefaultOnError_Scaled()
    {
      Aver.AreEqual(-5.7898M, Overpunch.ToDecimal("zgyug", 1_0000, -5.7898M, Data.ConvertErrorHandling.ReturnDefault));
    }

    [Run]
    [Aver.Throws(typeof(AzosException), "not parsable")]
    public void ToDecimal_ThrowOnError()
    {
      Overpunch.ToDecimal("zgyug", 10,  -5.23M, Data.ConvertErrorHandling.Throw);
    }

  }
}
