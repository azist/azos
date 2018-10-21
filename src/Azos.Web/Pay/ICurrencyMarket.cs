
using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Financial;

namespace Azos.Web.Pay
{
  /// <summary>
  /// Represents a market that can convert buy/sell currencies
  /// </summary>
  public interface ICurrencyMarket
  {
     /// <summary>
     /// Returns conversion rate for source->target conversion.
     /// rateTable is the name of the rates set, if omitted or not found then default conv rates will be used
     /// </summary>
     Amount ConvertCurrency(string rateTable, Amount from, string targetCurrencyISO);
  }

  public interface ICurrencyMarketImplementation : ICurrencyMarket, IApplicationComponent, IDisposable, IConfigurable
  {

  }

}
