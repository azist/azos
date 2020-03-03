/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Instrumentation;
using Azos.Serialization.Arow;
using Azos.Serialization.BSON;

namespace Azos.Platform.Instrumentation
{
  [Serializable]
  public abstract class OSLongGauge : LongGauge
  {
    protected OSLongGauge(string src, long value) : base(src, value) { }
  }

  [Serializable]
  public abstract class OSDoubleGauge : DoubleGauge
  {
    protected OSDoubleGauge(string src, double value) : base(src, value) { }
  }


  [Serializable]
  [Arow("252559C9-65E6-4EBD-8BE4-8A063D0E3FD0")]
  [BSONSerializable("8CAEA169-881F-43E4-92C8-6D774E913BB4")]
  public class CPUUsage : OSLongGauge, ICPUInstrument
  {
    protected CPUUsage(string src, long value) : base(src, value) { }

    public static void Record(IInstrumentation inst, long value, string src = null)
    {
      if (inst!=null && inst.Enabled) inst.Record(new CPUUsage(src, value));
    }


    public override string Description { get { return "CPU Usage %"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_PERCENT; } }


    protected override Datum MakeAggregateInstance()
    {
      return new CPUUsage(this.Source, 0);
    }
  }

  [Serializable]
  [Arow("4E2AEC33-36E3-4DC9-9B2C-A5703ACE5D6E")]
  [BSONSerializable("F87CE5FF-12AE-4FBA-83A7-479AB63E0C07")]
  public class RAMUsage : OSLongGauge, IMemoryInstrument
  {
    protected RAMUsage(long value) : base(null, value) { }

    public static void Record(IInstrumentation inst, long value)
    {
      if (inst!=null && inst.Enabled) inst.Record(new RAMUsage(value));
    }


    public override string Description { get { return "RAM Usage %"; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_PERCENT; } }


    protected override Datum MakeAggregateInstance()
    {
      return new RAMUsage(0);
    }
  }

  [Serializable]
  [Arow("15A6B78B-1511-4C5C-92E0-87A27317C3C2")]
  [BSONSerializable("C94378D4-8334-496D-AB28-ADF620071E97")]
  public class AvailableRAM : OSLongGauge, IMemoryInstrument
  {
    protected AvailableRAM(string src, long value) : base(src, value) { }

    public static void Record(IInstrumentation inst, long value, string src = null)
    {
      if (inst!=null && inst.Enabled)  inst.Record(new AvailableRAM(src, value));
    }


    public override string Description { get { return "Available RAM mb."; } }
    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MB; } }


    protected override Datum MakeAggregateInstance()
    {
      return new AvailableRAM(this.Source, 0);
    }
  }
}
