
using System;

namespace Azos.Instrumentation
{
  /// <summary>
  /// Represents a base for gauges - events of measurement of some values
  /// </summary>
  public abstract class Gauge : Datum
  {
    protected Gauge() : base() { }
    protected Gauge(string source) : base(source) { }
    protected Gauge(string source, DateTime utcDateTime) : base(source, utcDateTime) {}
  }
}
