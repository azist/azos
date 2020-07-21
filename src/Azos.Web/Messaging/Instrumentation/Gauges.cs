/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Instrumentation;
using Azos.Serialization.Arow;
using Azos.Serialization.BSON;

namespace Azos.Web.Messaging.Instrumentation
{
  [Serializable]
  public abstract class MessagingSinkLongGauge : LongGauge, IWebInstrument
  {
    protected MessagingSinkLongGauge(string source, long value) : base(source, value) { }
  }

  [Serializable]
  [Arow("090A8785-0764-4CBD-9B92-47D7A38C9681")]
  public class MessagingSinkCount : MessagingSinkLongGauge
  {
    public MessagingSinkCount(string source, long value) : base(source, value) { }

    public static void Record(IInstrumentation inst, string source, long value)
    {
      if (inst!=null && inst.Enabled)
        inst.Record(new MessagingSinkCount(source, value));
    }

    protected override Datum MakeAggregateInstance()
    {
      return new MessagingSinkCount(this.Source, 0);
    }

    public override string Description { get { return "Messages count"; } }
    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_MESSAGE; } }
  }

  [Serializable]
  [Arow("F3DC839B-0538-43AF-98DF-22935A8073B8")]
  public class MessagingFallbackCount : MessagingSinkLongGauge
  {
    public MessagingFallbackCount(string source, long value) : base(source, value) { }

    public static void Record(IInstrumentation inst, string source, long value)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new MessagingFallbackCount(source, value));
    }

    protected override Datum MakeAggregateInstance()
    {
      return new MessagingFallbackCount(this.Source, 0);
    }

    public override string Description { get { return "Fallbacks count"; } }
    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_MESSAGE; } }
  }

  [Serializable]
  [Arow("9BB4AFCB-EC6E-4B59-807F-1F43EA371D8D")]
  public class MessagingSinkErrorCount : MessagingSinkLongGauge
  {
    protected MessagingSinkErrorCount(string source, long value) : base(source, value) { }

    public static void Record(IInstrumentation inst, string source, long value)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new MessagingSinkErrorCount(source, value));
    }

    public override string Description { get { return "Messages error count"; } }
    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_ERROR; } }

    protected override Datum MakeAggregateInstance()
    {
      return new MessagingSinkErrorCount(this.Source, 0);
    }
  }

  [Serializable]
  [Arow("1C1C927F-53B3-4EA3-A522-B384ACC5D0E8")]
  public class MessagingFallbackErrorCount : MessagingSinkLongGauge
  {
    protected MessagingFallbackErrorCount(string source, long value) : base(source, value) { }

    public static void Record(IInstrumentation inst, string source, long value)
    {
      if (inst != null && inst.Enabled)
        inst.Record(new MessagingFallbackErrorCount(source, value));
    }

    public override string Description { get { return "Fallbacks error count"; } }
    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_ERROR; } }

    protected override Datum MakeAggregateInstance()
    {
      return new MessagingFallbackErrorCount(this.Source, 0);
    }
  }
}
