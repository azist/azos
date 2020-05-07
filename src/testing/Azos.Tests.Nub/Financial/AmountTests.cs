/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Financial;
using Azos.Serialization.JSON;
using Azos.Scripting;
using static Azos.Aver.ThrowsAttribute;

namespace Azos.Tests.Nub.Financial
{
  [Runnable]
  public class AmountTests
  {
    [Run]
    public void ToString_1()
    {
        var amt = new Amount("usd", 12.45M);

        Console.WriteLine(amt);
        Aver.AreEqual("12.45:usd", amt.ToString());
    }

    [Run]
    public void ToString_2()
    {
        var amt = new Amount("usd", -12.45M);

        Console.WriteLine(amt);
        Aver.AreEqual("-12.45:usd", amt.ToString());
    }

    [Run]
    public void ToString_3()
    {
        var amt = new Amount("usd", -122M);

        Console.WriteLine(amt);
        Aver.AreEqual("-122:usd", amt.ToString());
    }


    [Run]
    public void Equal()
    {
        Aver.IsTrue( new Amount("usd", 10M)  ==  new Amount("usd", 10M) );
    }

    [Run]
    public void NotEqual()
    {
        Aver.IsTrue( new Amount("usd", 102.11M)  !=  new Amount("usd", 100.12M) );
    }

    [Run]
    public void Less()
    {
        Aver.IsTrue( new Amount("usd", 10M)  <  new Amount("usd", 22.12M) );
    }

    [Run]
    public void Greater()
    {
        Aver.IsTrue( new Amount("usd", 102.11M)  >  new Amount("usd", 100.12M) );
    }

    [Run]
    public void LessOrEqual()
    {
        Aver.IsTrue( new Amount("usd", 22.12M)  <=  new Amount("usd", 22.12M) );
    }

    [Run]
    public void GreaterOrEqual()
    {
        Aver.IsTrue( new Amount("usd", 102.11M)  >=  new Amount("usd", 102.11M) );
    }



    [Run]
    public void Add()
    {
        Aver.AreEqual( new Amount("usd", 111.12M),   new Amount("usd", 10M) + new Amount("usd", 101.12M) );
    }

    [Run]
    public void Subtract()
    {
        Aver.AreEqual( new Amount("usd", -91.12M),   new Amount("usd", 10M) - new Amount("usd", 101.12M) );
    }

    [Run]
    [Aver.Throws(typeof(FinancialException), Message="different currencies", MsgMatch = MatchType.Contains)]
    public void Add_2()
    {
        var r = new Amount("sas", 10M) + new Amount("usd", 101.12M);
    }

    [Run]
    [Aver.Throws(typeof(FinancialException), Message="different currencies", MsgMatch = MatchType.Contains)]
    public void Subtract_2()
    {
        var r = new Amount("sas", 10M) - new Amount("usd", 101.12M);
    }



    [Run]
    public void Mul()
    {
        Aver.AreEqual( new Amount("usd", 100.10M),   new Amount("usd", 20.02M) * 5 );
        Aver.AreEqual( new Amount("usd", 100.10M),   5 * new Amount("usd", 20.02M) );

        Aver.AreEqual( new Amount("usd", 100.10M),   new Amount("usd", 20.02M) * 5d );
        Aver.AreEqual( new Amount("usd", 100.10M),   5d * new Amount("usd", 20.02M) );

        Aver.AreEqual( new Amount("usd", 100.10M),   new Amount("usd", 20.02M) * 5M );
        Aver.AreEqual( new Amount("usd", 100.10M),   5M * new Amount("usd", 20.02M) );

    }


    [Run]
    public void Div()
    {
        Aver.AreEqual( new Amount("usd", 20M),   new Amount("usd", 100M) / 5 );

        Aver.AreEqual( new Amount("usd", 20M),   new Amount("usd", 100M) / 5d );

        Aver.AreEqual( new Amount("usd", 20M),   new Amount("usd", 100M) / 5M );

    }


    [Run]
    public void Compare()
    {
        Aver.IsTrue( new Amount("usd", 100.12M).CompareTo( new Amount("usd", 200m)) < 0);
        Aver.IsTrue( new Amount("usd", 200.12M).CompareTo( new Amount("usd", 100m)) > 0);
        Aver.IsTrue( new Amount("usd", 200.12M).CompareTo( new Amount("usd", 200.12m)) == 0);

    }

    [Run]
    public void IsSameCurrency()
    {
        Aver.IsTrue( new Amount("usd", 100.12M).IsSameCurrencyAs( new Amount("usd", 200m)));
        Aver.IsTrue( new Amount("USd", 100.12M).IsSameCurrencyAs( new Amount("usD", 200m)));
        Aver.IsTrue( new Amount("USd ", 100.12M).IsSameCurrencyAs( new Amount("   usD ", 200m)));
        Aver.IsFalse( new Amount("usd", 100.12M).IsSameCurrencyAs( new Amount("eur", 200m)));

    }


    [Run]
    public void JSON()
    {
        var data = new {name="aaa", amount=new Amount("usd", 1234.12M)};
        var json = data.ToJson();

        Console.WriteLine(json);

        Aver.AreEqual(@"{""amount"":{""iso"":""usd"",""val"":1234.12},""name"":""aaa""}", json);

    }


    [Run]
    public void Parse_1()
    {
        var a = Amount.Parse("123:usd");
        Aver.AreEqual(new Amount("usd", 123m), a);
    }

    [Run]
    public void Parse_2()
    {
        var a = Amount.Parse("-123.12:usd");
        Aver.AreEqual(new Amount("usd", -123.12m), a);
    }

    [Run]
    public void Parse_3()
    {
      var a = Amount.Parse("-123.12");
      Aver.AreEqual(new Amount(null, -123.12m), a);
    }

    [Run]
    [Aver.Throws(typeof(FinancialException), Message="parse")]
    public void Parse_4()
    {
        var a = Amount.Parse("-1 23.12");
    }

    [Run]
    [Aver.Throws(typeof(FinancialException), Message="parse")]
    public void Parse_5()
    {
        var a = Amount.Parse(":-123.12");
    }


    [Run]
    [Aver.Throws(typeof(FinancialException), Message="parse")]
    public void Parse_6()
    {
        var a = Amount.Parse(":123");
    }

    [Run]
    public void Parse_7()
    {
        var a = Amount.Parse("-123.12 : uah");
        Aver.AreEqual(new Amount("UAH", -123.12m), a);
    }

    [Run]
    [Aver.Throws(typeof(FinancialException), Message="parse")]
    public void Parse_8()
    {
        Amount.Parse("-123.12 :");
    }

    [Run]
    [Aver.Throws(typeof(FinancialException), Message = "parse")]
    public void Parse_9()
    {
      Amount.Parse("123.12:ooooooooooooooooooooooooooooooooooooooooooooooooooo");
    }

    [Run]
    public void TryParse_1()
    {
        Amount a;
        var parsed = Amount.TryParse("123:usd", out a);
        Aver.IsTrue( parsed );
        Aver.AreEqual( new Amount("usd", 123), a);
    }

    [Run]
    public void TryParse_2()
    {
        Amount a;
        var parsed = Amount.TryParse("-1123:usd", out a);
        Aver.IsTrue( parsed );
        Aver.AreEqual( new Amount("usd", -1123M), a);
    }

    [Run]
    public void TryParse_3()
    {
        Amount a;
        var parsed = Amount.TryParse("-1123:eur", out a);
        Aver.IsTrue( parsed );
        Aver.AreEqual( new Amount("eur", -1123M), a);
    }

    [Run]
    public void TryParse_4()
    {
        Amount a;
        var parsed = Amount.TryParse("-1123:rub", out a);
        Aver.IsTrue( parsed );
        Aver.AreEqual( new Amount("rub", -1123M), a);
    }

    [Run]
    public void TryParse_5()
    {
        Amount a;
        var parsed = Amount.TryParse("-11 23", out a);
        Aver.IsFalse( parsed );
    }

    [Run]
    public void TryParse_6()
    {
        Amount a;
        var parsed = Amount.TryParse(":1123", out a);
        Aver.IsFalse( parsed );
    }

    [Run]
    public void TryParse_7()
    {
        Amount a;
        var parsed = Amount.TryParse("", out a);
        Aver.IsFalse( parsed );
    }

    [Run]
    public void TryParse_8()
    {
        Amount a;
        var parsed = Amount.TryParse("aaa:bbb", out a);
        Aver.IsFalse( parsed );
    }

    [Run]
    public void TryParse_9()
    {
        Amount a;
        var parsed = Amount.TryParse("-1123 :gbp", out a);
        Aver.IsTrue( parsed );
        Aver.AreEqual( new Amount("gbp", -1123M), a);
    }

    [Run]
    public void TryParse_10()
    {
        Amount a;
        var parsed = Amount.TryParse("-1123 :  uah", out a);
        Aver.IsTrue( parsed );
        Aver.AreEqual( new Amount("UAH", -1123M), a);
    }

    [Run]
    public void TryParse_11()
    {
      Amount a;
      Aver.IsFalse(Amount.TryParse(":usd", out a));
      Aver.IsFalse(Amount.TryParse(":usduyfuyiuyoiuyoiuyoiuyiouyiouyiuyuiyiuy", out a));
      Aver.IsFalse(Amount.TryParse("123 :", out a));
      Aver.IsFalse(Amount.TryParse("123 :    ooooooooooooooo", out a));
      Aver.IsTrue(Amount.TryParse("123", out a));
      Aver.IsTrue(Amount.TryParse("-123", out a));
    }

  }
}