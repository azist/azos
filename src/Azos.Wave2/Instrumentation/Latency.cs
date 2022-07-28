/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Instrumentation;
using Azos.Serialization.Bix;

namespace Azos.Wave.Instrumentation
{
  /// <summary>
  /// Measures API execution times
  /// </summary>
  [Serializable, Bix("AF9BA31D-3555-4F9F-9498-8F1BF197BEAA")]
  public sealed class ApiLatency : WaveLongGauge, INetInstrument, IWebInstrument
  {
    /// <summary>
    /// Emits the Api latency reading and ApiCallEvent
    /// </summary>
    public static void EmitApiCall(IInstrumentation inst, string ctxName, string uri, long msLatency, bool emitEvent = true)
    {
      if (inst == null || !inst.Enabled) return;
      var src = $"{ctxName}::{uri}";
      var datum = new ApiLatency(src, msLatency);
      inst.Record(datum);

      datum = new ApiLatency(UNSPECIFIED_SOURCE, msLatency);//general
      inst.Record(datum);

      if (emitEvent)
      {
        inst.Record(new ApiCallEvent(src));
        inst.Record(new ApiCallEvent(UNSPECIFIED_SOURCE));
      }
    }

    internal ApiLatency(string src, long value) : base(src, value) { }

    public override string Description => "Measures API execution times";
    public override string ValueUnitName => Azos.CoreConsts.UNIT_NAME_MSEC;

    protected override Datum MakeAggregateInstance() => new ApiLatency(Source, 0);
  }

  /// <summary>
  /// Indicates an API call occurrence
  /// </summary>
  [Serializable, Bix("{A9652F56-740A-4581-BED8-454F0C5A4F0D}")]
  public sealed class ApiCallEvent : WaveEvent
  {
    internal ApiCallEvent(string src) : base(src) { }

    public override string Description => "Indicates an API call occurrence";
    public override string ValueUnitName => Azos.CoreConsts.UNIT_NAME_TIME;

    protected override Datum MakeAggregateInstance() => new ApiCallEvent(Source);
  }
}
