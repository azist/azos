/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Financial.Market;


namespace Azos.WinForms.Controls.ChartKit.Temporal
{

  /// <summary>
  /// Stores Candle time series data
  /// </summary>
  public class CandleTimeSeries : TimeSeries<CandleSample>
  {
      public CandleTimeSeries(string name, int order, TimeSeries parent = null)
              : base(name, order, parent)
      {
      }
  }

}
