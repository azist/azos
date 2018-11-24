/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;


using Azos.Instrumentation;
using Azos.Serialization.BSON;

namespace Azos.Time.Instrumentation
{
  [Serializable]
  public abstract class TimeInstrumentationLongGauge : LongGauge, ISchedulingInstrument
  {
    protected TimeInstrumentationLongGauge(long value) : base(null, value) { }
  }

  [Serializable]
  [BSONSerializable("31CFA04B-F61B-4960-A6F6-9E1CF83C698A")]
  public class EventCount : TimeInstrumentationLongGauge
  {
    protected EventCount(long value) : base(value) { }

    public static void Record(IInstrumentation inst, long value)
    {
      if (inst!=null && inst.Enabled) inst.Record(new EventCount(value));
    }


    public override string Description { get { return "Timer event count"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_EVENT; } }


    protected override Datum MakeAggregateInstance()
    {
      return new EventCount(0);
    }
  }

  [Serializable]
  [BSONSerializable("B9D1E7D0-1204-438D-BDAA-EE66963BA84F")]
  public class FiredEventCount : TimeInstrumentationLongGauge
  {
    protected FiredEventCount(long value) : base(value) { }

    public static void Record(IInstrumentation inst, long value)
    {
      if (inst!=null && inst.Enabled) inst.Record(new FiredEventCount(value));
    }


    public override string Description { get { return "Count of timer events that have fired since last measurement"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_EVENT; } }


    protected override Datum MakeAggregateInstance()
    {
      return new FiredEventCount(0);
    }
  }
}
