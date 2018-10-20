
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
