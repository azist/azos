/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
  
using System;
using Azos.Scripting;
using Azos.Web.Pay;
using Azos.Financial;
using Azos.Conf;

namespace Azos.Tests.Unit.Web.Pay
{
  [Runnable]
  public class ConfigBasedCurrencyMarketTests
  {
    #region Consts

      private const string CURRENCY_USD = "usd";
      private const string CURRENCY_RUB = "rub";

      private const string NO_RATE_TABLE = null;
      private const string RATE_TABLE_SAFE = "safe";

      private const int RATE_USD_TO_RUB = 70;
      private readonly Amount USD1 = new Amount(CURRENCY_USD, 1M);

    #endregion

    #region Public

      [Run]
      [Aver.Throws(typeof(PaymentException))]
      public void CurrencyMarket_NoConfig()
      {
        var market = new ConfigBasedCurrencyMarket();
        market.ConvertCurrency(NO_RATE_TABLE, USD1, CURRENCY_RUB);
      }

      [Run]
      [Aver.Throws(typeof(PaymentException))]
      public void CurrencyMarket_NoCurrencyMarketSection()
      {
        var config = "payment-processing {}".AsLaconicConfig();

        var market = new ConfigBasedCurrencyMarket(config);
        Convert(config, USD1, CURRENCY_RUB);
      }

      [Run]
      [Aver.Throws(typeof(PaymentException))]
      public void CurrencyMarket_NoRequiredDefaultRate()
      {
        var config = "payment-processing { usd=eur { rate=1.1 } }".AsLaconicConfig();

        Convert(config, USD1, CURRENCY_RUB);
      }

      [Run]
      [Aver.Throws(typeof(PaymentException))]
      public void CurrencyMarket_NoRequiredRateInTable()
      {
        var config = @"payment-processing { 
          usd=rub { rate=70 } 
          tables {
            safe { usd=eur { rate=1.1 } } 
          }
        }".AsLaconicConfig();

        Convert(config, RATE_TABLE_SAFE, USD1, CURRENCY_RUB);
      }

      [Run]
      public void CurrencyMarket_DefaultRequiredRate()
      {
        var config = "payment-processing { usd=rub { rate=70 } }".AsLaconicConfig();

        var result = Convert(config, USD1, CURRENCY_RUB);

        Aver.AreEqual(CURRENCY_RUB, result.CurrencyISO);
        Aver.AreEqual(USD1.Value * RATE_USD_TO_RUB, result.Value);
      }

      [Run]
      public void CurrencyMarket_FallbackToDefaultRate()
      {
        var config = @"payment-processing { 
          usd=rub { rate=70 } 
          tables {
            not-safe { usd=eur { rate=1.1 } } 
          }
        }".AsLaconicConfig();

        var result = Convert(config, RATE_TABLE_SAFE, USD1, CURRENCY_RUB);

        Aver.AreEqual(CURRENCY_RUB, result.CurrencyISO);
        Aver.AreEqual(USD1.Value * RATE_USD_TO_RUB, result.Value);
      }

    #endregion

    #region .pvt

      private Amount Convert(ConfigSectionNode config, Amount amount, string currencyISO)
      {
        return Convert(config, NO_RATE_TABLE, amount, currencyISO);
      }

      private Amount Convert(ConfigSectionNode config, string rateTable, Amount amount, string currencyISO)
      {
        var market = new ConfigBasedCurrencyMarket(config);
        return market.ConvertCurrency(rateTable, amount, currencyISO);
      }

    #endregion
  }
}
