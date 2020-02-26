/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Instrumentation;
using Azos.Serialization.Arow;
using Azos.Serialization.BSON;

namespace Azos.Log.Instrumentation
{

  [Serializable]
  public abstract class LogLongGauge : LongGauge, IInstrumentationInstrument
  {
    protected LogLongGauge(string source, long value) : base(source, value) { }
  }

  [Serializable]
  [Arow("21420BC2-A2F4-461D-A087-59F5AC4D843D")]
  [BSONSerializable("D33255A6-549A-4D65-A74D-51B47F1B94DA")]
  public class LogMsgQueueSize : LogLongGauge, IMemoryInstrument
  {
    protected LogMsgQueueSize(string source, long value) : base(source, value) { }

    public static void Record(IInstrumentation inst, string source, long value)
    {
      if (inst!=null && inst.Enabled)
        inst.Record(new LogMsgQueueSize(source, value));
    }


    public override string Description { get { return "Log message count in queue"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MESSAGE; } }


    protected override Datum MakeAggregateInstance()
    {
      return new LogMsgQueueSize(this.Source, 0);
    }
  }


  [Serializable]
  [Arow("E0E7D9AE-0165-4F81-95C9-113B297C625D")]
  [BSONSerializable("DABE72B4-8DF5-4299-BA1C-E0D8AAD3F983")]
  public class LogMsgCount : LogLongGauge, IMemoryInstrument
  {
    protected LogMsgCount(string source, long value) : base(source, value) { }

    public static void Record(IInstrumentation inst, string source, long value)
    {
      if (inst!=null && inst.Enabled)
        inst.Record(new LogMsgCount(source, value));
    }


    public override string Description { get { return "Log message count"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MESSAGE; } }


    protected override Datum MakeAggregateInstance()
    {
      return new LogMsgCount(this.Source, 0);
    }
  }
}
