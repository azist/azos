
using System;

using Azos.Apps;
using Azos.Instrumentation;
using Azos.Serialization.BSON;

namespace Azos.Instrumentation.Self
{

  [Serializable]
  public abstract class SelfInstrumentationLongGauge : LongGauge, IInstrumentationInstrument
  {
    protected SelfInstrumentationLongGauge(long value) : base(null, value) { }
  }

  [Serializable]
  [BSONSerializable("4FCE71FC-4082-4783-AB78-05F541DB9213")]
  public class RecordCount : SelfInstrumentationLongGauge, IMemoryInstrument
  {
    protected RecordCount(long value) : base(value) { }

    public static void Record(long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new RecordCount(value));
    }

    public override string Description { get { return "Datum record count"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_RECORD; } }

    protected override Datum MakeAggregateInstance() { return new RecordCount(0); }
  }

  [Serializable]
  [BSONSerializable("FA356440-4E5B-4504-A887-97FB9AD66194")]
  public class RecordLoad : SelfInstrumentationLongGauge, IMemoryInstrument
  {
    protected RecordLoad(long value) : base(value) { }

    public static void Record(long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new RecordLoad(value));
    }

    public override string Description { get { return "Instrumentation load percentage: recordCount / maxRecordCount"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_PERCENT; } }

    protected override Datum MakeAggregateInstance() { return new RecordLoad(0); }
  }

  [Serializable]
  [BSONSerializable("947A802D-42BF-424E-B294-120C47A26E12")]
  public class ProcessingInterval : SelfInstrumentationLongGauge
  {
    protected ProcessingInterval(long value) : base(value) { }

    public static void Record(long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new ProcessingInterval(value));
    }

    public override string Description { get { return "Instrumentation processing interval in milliseconds"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MSEC; } }

    protected override Datum MakeAggregateInstance() { return new ProcessingInterval(0); }
  }


  [Serializable]
  [BSONSerializable("9D3B3DE2-4CA5-4FC7-9827-D79D26E39F80")]
  public class BufferMaxAge : SelfInstrumentationLongGauge, IMemoryInstrument
  {
    protected BufferMaxAge(long value) : base(value) { }

    public static void Record(long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new BufferMaxAge(value));
    }

    public override string Description { get { return "Age of the oldest datum in result buffer in seconds"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_SEC; } }

    protected override Datum MakeAggregateInstance() { return new BufferMaxAge(0); }
  }
}
