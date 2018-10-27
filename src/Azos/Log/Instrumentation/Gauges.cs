/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Instrumentation;
using Azos.Serialization.BSON;

namespace Azos.Log.Instrumentation
{

  [Serializable]
  public abstract class LogLongGauge : LongGauge, IInstrumentationInstrument
  {
    protected LogLongGauge(string source, long value) : base(source, value) { }
  }

  [Serializable]
  [BSONSerializable("DABE72B4-8DF5-4299-BA1C-E0D8AAD3F983")]
  public class LogMsgCount : LogLongGauge, IMemoryInstrument
  {
    protected LogMsgCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
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
