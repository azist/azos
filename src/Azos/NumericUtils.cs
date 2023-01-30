/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos
{
  /// <summary>
  /// Provides helper utility methods for working with numbers
  /// </summary>
  public static class NumericUtils
  {
    /// <summary>
    /// Ensures that a float value is between the low and high bounds inclusive of the edge points
    /// </summary>
    public static float MinMax(float min, float value, float max)
    {
        if (min > max) throw new AzosException(StringConsts.ARGUMENT_ERROR + "MinMax(min > max)");
        if (value < min) value = min;
        if (value > max) value = max;
        return value;
    }

    /// <summary>
    /// Ensures that a double value is between the low and high bounds inclusive of the edge points
    /// </summary>
    public static double MinMax(double min, double value, double max)
    {
      if (min > max) throw new AzosException(StringConsts.ARGUMENT_ERROR + "MinMax(min > max)");
      if (value < min) value = min;
      if (value > max) value = max;
      return value;
    }

    /// <summary>
    /// Ensures that a decimal value is between the low and high bounds inclusive of the edge points
    /// </summary>
    public static decimal MinMax(decimal min, decimal value, decimal max)
    {
      if (min > max) throw new AzosException(StringConsts.ARGUMENT_ERROR + "MinMax(min > max)");
      if (value < min) value = min;
      if (value > max) value = max;
      return value;
    }

    /// <summary>
    /// Ensures that a float value is between the low and high bounds inclusive of the edge points
    /// </summary>
    public static float KeepBetween(this float value, float min, float max)
     => MinMax(min, value, max);

    /// <summary>
    /// Ensures that a double value is between the low and high bounds inclusive of the edge points
    /// </summary>
    public static double KeepBetween(this double value, double min, double max)
     => MinMax(min, value, max);

    /// <summary>
    /// Ensures that a decimal value is between the low and high bounds inclusive of the edge points
    /// </summary>
    public static decimal KeepBetween(this decimal value, decimal min, decimal max)
     => MinMax(min, value, max);

    /// <summary>
    /// Ensures that a float value is not below the indicated minimum value
    /// </summary>
    public static float AtMinimum(this float value, float min) => value < min ? min : value;

    /// <summary>
    /// Ensures that a double value is not below the indicated minimum value
    /// </summary>
    public static double AtMinimum(this double value, double min) => value < min ? min : value;

    /// <summary>
    /// Ensures that a decimal value is not below the indicated minimum value
    /// </summary>
    public static decimal AtMinimum(this decimal value, decimal min) => value < min ? min : value;


    /// <summary>
    /// Changes the number by a random margin of up to the specified pct
    /// </summary>
    public static float ChangeByRndPct(this float value, float pct)
    {
      return value + (float)((value * pct) * Ambient.Random.NextRandomDouble);
    }

    /// <summary>
    /// Changes the number by a random margin of up to the specified pct
    /// </summary>
    public static double ChangeByRndPct(this double value, double pct)
    {
      return value + ((value * pct) * Ambient.Random.NextRandomDouble);
    }

    /// <summary>
    /// Changes the number by a random margin of up to the specified pct
    /// </summary>
    public static decimal ChangeByRndPct(this decimal value, decimal pct)
    {
      return value + ((value * pct) * (decimal)Ambient.Random.NextRandomDouble);
    }
  }
}
