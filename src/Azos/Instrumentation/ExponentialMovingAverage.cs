/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Instrumentation
{
  /// <summary>
  /// Facilitates EMA calculations
  /// </summary>
  public static class EmaUtils
  {
    /// <summary>
    /// Calculates Exponential Moving Average
    /// </summary>
    public static double Ema(this double avg, double sample, double factor) => (factor * sample) + ((1.0d - factor) * avg);

    /// <summary>
    /// Calculates Exponential Moving Average
    /// </summary>
    public static decimal Ema(this decimal avg, decimal sample, decimal factor) => (factor * sample) + ((1.0m - factor) * avg);

    /// <summary>
    /// Calculates Exponential Moving Average
    /// </summary>
    public static long Ema(this long avg, long sample, double factor) => (long)((factor * sample) + ((1.0d - factor) * avg));
  }


  /// <summary>
  /// Implements a simple EMA (Exponential Moving Average) vector for double values
  /// </summary>
  public struct EmaDouble
  {
    /// <summary>
    /// Modifies EMA value in-place by adding a new sample, e.g.:  `EmaDouble.AddNext(ref m_AverageSize, 12.5d);`
    /// </summary>
    public static void AddNext(ref EmaDouble ema, double sample)
    {
      ema = ema.AddNext(sample);
    }

    /// <summary>
    /// Returns a new EMA value by adding a new sample, e.g.:  `var worldAvgSize = myAvgSize.AddNext(5.1d);`
    /// </summary>
    public EmaDouble AddNext(double sample)
    {
      var avg = (Factor * sample) + ((1.0d - Factor) * Average);
      return new EmaDouble(Factor, avg, sample);
    }

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    public EmaDouble(double factor, double avg = 0d, double sample = 0d)
    {
      Factor = factor;
      Average = avg;
      Sample = sample;
    }

    /// <summary>
    /// Defines how much smoothing gets reflected in the average value - the lower the number the more smoothing is done.
    /// </summary>
    public readonly double Factor;

    /// <summary> Calculated average value using exponential moving formula </summary>
    public readonly double Average;

    /// <summary> Last supplied sample (already applied into average value) </summary>
    public readonly double Sample;
  }


  /// <summary>
  /// Implements a simple EMA (Exponential Moving Average) vector for decimal/money values
  /// </summary>
  public struct EmaDecimal
  {
    /// <summary>
    /// Modifies EMA value in-place by adding a new sample, e.g.:  `EmaDouble.AddNext(ref m_AverageSize, 12.5d);`
    /// </summary>
    public static void AddNext(ref EmaDecimal ema, decimal sample)
    {
      ema = ema.AddNext(sample);
    }

    /// <summary>
    /// Returns a new EMA value by adding a new sample, e.g.:  `var worldAvgSize = myAvgSize.AddNext(5.1d);`
    /// </summary>
    public EmaDecimal AddNext(decimal sample)
    {
      var avg = (Factor * sample) + ((1.0m - Factor) * Average);
      return new EmaDecimal(Factor, avg, sample);
    }

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    public EmaDecimal(decimal factor, decimal avg = 0m, decimal sample = 0m)
    {
      Factor = factor;
      Average = avg;
      Sample = sample;
    }

    /// <summary>
    /// Defines how much smoothing gets reflected in the average value - the lower the number the more smoothing is done.
    /// </summary>
    public readonly decimal Factor;

    /// <summary> Calculated average value using exponential moving formula </summary>
    public readonly decimal Average;

    /// <summary> Last supplied sample (already applied into average value) </summary>
    public readonly decimal Sample;
  }
}
