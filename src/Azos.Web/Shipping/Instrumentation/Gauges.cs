/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Instrumentation;
using Azos.Serialization.BSON;

namespace Azos.Web.Shipping.Instrumentation
{
  [Serializable]
  public abstract class ShippingLongGauge : LongGauge, IFinancialLogic, IWebInstrument
  {
    protected ShippingLongGauge(string source, long value) : base(source, value) { }
  }

  [Serializable]
  [BSONSerializable("811BEE94-E9B8-4A2D-A0F7-592631A69C97")]
  public class LabelCount : ShippingLongGauge
  {
    protected LabelCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new LabelCount(source, value));
    }


    public override string Description { get { return "Labels count"; } }
    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance()
    {
      return new LabelCount(this.Source, 0);
    }
  }

  [Serializable]
  [BSONSerializable("F52805B2-BB94-4AEF-AE65-BD23AE0A0DC6")]
  public class LabelErrorCount : ShippingLongGauge
  {
    protected LabelErrorCount(string source, long value) : base(source, value) { }

    public static void Record(string source, long value)
    {
      var inst = App.Instrumentation;
      if (inst.Enabled)
        inst.Record(new LabelErrorCount(source, value));
    }


    public override string Description { get { return "Label creation error count"; } }
    public override string ValueUnitName { get { return Azos.CoreConsts.UNIT_NAME_ERROR; } }

    protected override Datum MakeAggregateInstance()
    {
      return new LabelErrorCount(this.Source, 0);
    }
  }
}
