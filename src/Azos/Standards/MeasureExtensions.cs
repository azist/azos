/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


namespace Azos.Standards
{
  /// <summary>
  /// Various extensions for working with measures
  /// </summary>
  public static class MeasureExtensions
  {
    public static Distance ComposeDistance(this string a, string b)   => (Distance)a + (Distance)b;
    public static Distance In(this decimal v, Distance.UnitType unit) => new Distance(v, unit);
    public static Distance In(this double v, Distance.UnitType unit)  => new Distance((decimal)v, unit);
    public static Distance In(this float v, Distance.UnitType unit)   => new Distance((decimal)v, unit);
    public static Distance In(this int v, Distance.UnitType unit)     => new Distance((decimal)v, unit);
    public static Distance In(this long v, Distance.UnitType unit)    => new Distance(unit, v);
  }
}