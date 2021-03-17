/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Instrumentation
{
  /// <summary>
  /// Represents a base for gauges - events of value measurement (e.g. voltage in the wall outlet, vehicle speed, engine rpm etc..).
  /// The measurements can either be scalar (e.g. one number) or vector (e.g. human blood pressure pair)
  /// </summary>
  public abstract class Gauge : Datum
  {
    protected Gauge() : base() { }
    protected Gauge(string source) : base(source) { }
    protected Gauge(string source, DateTime utcDateTime) : base(source, utcDateTime) {}
  }
}
