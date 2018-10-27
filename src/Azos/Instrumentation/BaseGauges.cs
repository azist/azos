/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Financial;
using Azos.Serialization.BSON;

namespace Azos.Instrumentation
{
  /// <summary>
  /// Represents a general-purpose long integer measurement datum
  /// </summary>
  [Serializable]
  public abstract class LongGauge : Gauge
  {
    protected LongGauge(long value) : base() { m_Value = value; }

    protected LongGauge(string source, long value) : base(source) { m_Value = value; }

    protected LongGauge(string source, long value, DateTime utcDateTime) : base(source, utcDateTime) { m_Value = value; }

    private long m_Value;

    /// <summary>
    /// Gets gauge value
    /// </summary>
    public long Value { get { return m_Value; } }

    public override object ValueAsObject { get { return m_Value; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_UNSPECIFIED; } }

    public override void SerializeToBSON(BSONSerializer serializer, BSONDocument doc, IBSONSerializable parent, ref object context)
    {
      base.SerializeToBSON(serializer, doc, parent, ref context);
      doc.Add(BSON_FLD_VALUE, m_Value);
    }

    public override void DeserializeFromBSON(BSONSerializer serializer, BSONDocument doc, ref object context)
    {
      m_Value = doc.TryGetObjectValueOf(BSON_FLD_VALUE).AsLong();
      base.DeserializeFromBSON(serializer, doc, ref context);
    }

    [NonSerialized]
    private long m_Sum;

    protected override void AggregateEvent(Datum dat)
    {
      var dg = dat as LongGauge;
      if (dg == null) return;

      m_Sum += dg.Value;
    }

    protected override void SummarizeAggregation()
    {
      m_Value = m_Sum / m_Count;
    }
  }


  /// <summary>
  /// Represents a general-purpose double measurement datum
  /// </summary>
  [Serializable]
  public abstract class DoubleGauge : Gauge
  {
    protected DoubleGauge(string source, double value) : base(source) { m_Value = value; }

    protected DoubleGauge(string source, double value, DateTime utcDateTime) : base(source, utcDateTime) { m_Value = value; }

    private double m_Value;

    /// <summary>
    /// Gets gauge value
    /// </summary>
    public double Value { get { return m_Value; } }

    public override object ValueAsObject { get { return m_Value; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_UNSPECIFIED; } }

    public override void SerializeToBSON(BSONSerializer serializer, BSONDocument doc, IBSONSerializable parent, ref object context)
    {
      base.SerializeToBSON(serializer, doc, parent, ref context);
      doc.Add(BSON_FLD_VALUE, m_Value);
    }

    public override void DeserializeFromBSON(BSONSerializer serializer, BSONDocument doc, ref object context)
    {
      m_Value = doc.TryGetObjectValueOf(BSON_FLD_VALUE).AsDouble();
      base.DeserializeFromBSON(serializer, doc, ref context);
    }

    [NonSerialized]
    private double m_Sum;

    protected override void AggregateEvent(Datum dat)
    {
      var dg = dat as DoubleGauge;
      if (dg == null) return;

      m_Sum += dg.Value;
    }

    protected override void SummarizeAggregation()
    {
      if (m_Count > 0)
        m_Value = m_Sum / (double)m_Count;
    }
  }


  /// <summary>
  /// Represents a general-purpose decimal measurement datum
  /// </summary>
  [Serializable]
  public abstract class DecimalGauge : Gauge
  {
    protected DecimalGauge(string source, decimal value) : base(source) { m_Value = value; }

    protected DecimalGauge(string source, decimal value, DateTime utcDateTime) : base(source, utcDateTime) { m_Value = value; }

    private decimal m_Value;

    /// <summary>
    /// Gets gauge value
    /// </summary>
    public decimal Value { get { return m_Value; } }

    public override object ValueAsObject { get { return m_Value; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_UNSPECIFIED; } }

    public override void SerializeToBSON(BSONSerializer serializer, BSONDocument doc, IBSONSerializable parent, ref object context)
    {
      base.SerializeToBSON(serializer, doc, parent, ref context);
      doc.Set(DataDocConverter.Decimal_CLRtoBSON(BSON_FLD_VALUE, m_Value));
    }

    public override void DeserializeFromBSON(BSONSerializer serializer, BSONDocument doc, ref object context)
    {
      base.DeserializeFromBSON(serializer, doc, ref context);
      m_Value = DataDocConverter.Decimal_BSONtoCLR(doc[BSON_FLD_VALUE]);
    }

    [NonSerialized]
    private decimal m_Sum;

    protected override void AggregateEvent(Datum dat)
    {
      var dg = dat as DecimalGauge;
      if (dg == null) return;

      m_Sum += dg.Value;
    }

    protected override void SummarizeAggregation()
    {
      if (m_Count > 0)
        m_Value = m_Sum / (decimal)m_Count;
    }
  }

  /// <summary>
  /// Represents a general-purpose financial Amount measurement datum
  /// </summary>
  [Serializable]
  public abstract class AmountGauge : Gauge, IFinancialLogic
  {
    public const string CURRENCY_DELIM = "::";

    private static string makeSource(string source, Amount value)
    {
      var prefix = value.CurrencyISO + CURRENCY_DELIM;
      if (source == null || !source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        return prefix + source;
      else
        return source;
    }

    protected AmountGauge(string source, Amount value) : base(makeSource(source, value))
    {
      m_Value = value;
      m_Sum = new Amount(value.CurrencyISO, 0M);
    }

    protected AmountGauge(string source, Amount value, DateTime utcDateTime, bool skipSourceConstruction = false) : base(makeSource(source, value), utcDateTime)
    {
      m_Value = value;
      m_Sum = new Amount(value.CurrencyISO, 0M);
    }

    private Amount m_Value;

    /// <summary>
    /// Gets gauge value
    /// </summary>
    public Amount Value { get { return m_Value; } }

    public override object ValueAsObject { get { return m_Value; } }

    public override object PlotValue { get { return m_Value.Value; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_MONEY; } }

    public override void SerializeToBSON(BSONSerializer serializer, BSONDocument doc, IBSONSerializable parent, ref object context)
    {
      base.SerializeToBSON(serializer, doc, parent, ref context);
      doc.Set(DataDocConverter.Amount_CLRtoBSON(BSON_FLD_VALUE, m_Value));
    }

    public override void DeserializeFromBSON(BSONSerializer serializer, BSONDocument doc, ref object context)
    {
      base.DeserializeFromBSON(serializer, doc, ref context);
      m_Value = DataDocConverter.Amount_BSONtoCLR((BSONDocumentElement)doc[BSON_FLD_VALUE]);
    }

    [NonSerialized]
    private Amount m_Sum;

    protected override void AggregateEvent(Datum dat)
    {
      var dg = dat as AmountGauge;
      if (dg == null) return;

      m_Sum = m_Sum + dg.Value;
    }

    protected override void SummarizeAggregation()
    {
      if (m_Count > 0)
        m_Value = m_Sum / (double)m_Count;
    }
  }
}
