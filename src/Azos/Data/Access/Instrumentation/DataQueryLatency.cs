/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Instrumentation;
using Azos.Serialization.Bix;

namespace Azos.Data.Access.Instrumentation
{
  /// <summary>
  /// Measures data query execution times
  /// </summary>
  [Serializable, Bix("5EF35033-44FE-4D7B-9F15-D479EE94D20C")]
  public sealed class DataQueryLatency : LongGauge, INetInstrument, IDatabaseInstrument
  {
    /// <summary>
    /// Emits the latency reading
    /// </summary>
    public static void Emit(IInstrumentation inst, string ctxName, string queryName, long msLatency, string queryParam = null)
    {
      if (inst == null || !inst.Enabled) return;
      var src = $"{ctxName}::{queryName}::{queryParam}";

      var datum = new DataQueryLatency(src, msLatency);
      inst.Record(datum);

      datum = new DataQueryLatency(UNSPECIFIED_SOURCE, msLatency);//general
      inst.Record(datum);
    }

    internal DataQueryLatency(string src, long value) : base(src, value) { }

    public override string Description => "Measures data query execution time";
    public override string ValueUnitName => CoreConsts.UNIT_NAME_MSEC;

    protected override Datum MakeAggregateInstance() => new DataQueryLatency(Source, 0);
  }
}
