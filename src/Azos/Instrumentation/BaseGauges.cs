/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Financial;

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
    [Field, Field(isArow: true, backendName: "v")]
    public long Value { get { return m_Value; } }

    public override object ValueAsObject => m_Value;

    public override double? RefValue => m_Value;

    public override string ValueUnitName => CoreConsts.UNIT_NAME_UNSPECIFIED;

    [NonSerialized]
    private long m_Sum;

    protected override void AggregateOne(Datum dat)
    {
      var dg = dat as LongGauge;
      if (dg == null) return;

      m_Sum += dg.Value;
    }

    protected override void SummarizeAggregation()
    {
      m_Value = m_Sum / Count;
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
    [Field, Field(isArow: true, backendName: "v")]
    public double Value { get { return m_Value; } }

    public override object ValueAsObject => m_Value;

    public override double? RefValue => m_Value;

    public override string ValueUnitName => CoreConsts.UNIT_NAME_UNSPECIFIED;

    [NonSerialized]
    private double m_Sum;

    protected override void AggregateOne(Datum dat)
    {
      var dg = dat as DoubleGauge;
      if (dg == null) return;

      m_Sum += dg.Value;
    }

    protected override void SummarizeAggregation()
    {
      if (Count > 0)
        m_Value = m_Sum / (double)Count;
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
    [Field, Field(isArow: true, backendName: "v")]
    public decimal Value { get { return m_Value; } }

    public override object ValueAsObject => m_Value;

    public override double? RefValue => (double)m_Value;

    public override string ValueUnitName => CoreConsts.UNIT_NAME_UNSPECIFIED;

    [NonSerialized]
    private decimal m_Sum;

    protected override void AggregateOne(Datum dat)
    {
      var dg = dat as DecimalGauge;
      if (dg == null) return;

      m_Sum += dg.Value;
    }

    protected override void SummarizeAggregation()
    {
      if (Count > 0)
        m_Value = m_Sum / (decimal)Count;
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
      var prefix = value.ISO.Value + CURRENCY_DELIM;
      if (source == null || !source.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        return prefix + source;
      else
        return source;
    }

    protected AmountGauge(string source, Amount value) : base(makeSource(source, value))
    {
      m_Value = value;
      m_Sum = new Amount(value.ISO, 0M);
    }

    protected AmountGauge(string source, Amount value, DateTime utcDateTime, bool skipSourceConstruction = false) : base(makeSource(source, value), utcDateTime)
    {
      m_Value = value;
      m_Sum = new Amount(value.ISO, 0M);
    }

    private Amount m_Value;

    /// <summary>
    /// Gets gauge value
    /// </summary>
    [Field, Field(isArow: true, backendName: "v")]
    public Amount Value { get { return m_Value; } }

    public override object ValueAsObject => m_Value;

    public override object PlotValue => m_Value.Value;

    public override double? RefValue => (double)m_Value.Value;

    public override string ValueUnitName => CoreConsts.UNIT_NAME_MONEY;

    [NonSerialized]
    private Amount m_Sum;

    protected override void AggregateOne(Datum dat)
    {
      var dg = dat as AmountGauge;
      if (dg == null) return;

      m_Sum = m_Sum + dg.Value;
    }

    protected override void SummarizeAggregation()
    {
      if (Count > 0)
        m_Value = m_Sum / (double)Count;
    }
  }
}
