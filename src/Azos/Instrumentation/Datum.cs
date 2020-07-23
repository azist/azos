/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Azos.Data;
using Azos.Platform;
using Azos.Serialization.JSON;

namespace Azos.Instrumentation
{
  /// <summary>
  /// Base class for a single measurement events (singular: datum/plural: data) reported to instrumentation
  /// </summary>
  [Serializable]
  [Serialization.Bix.BixJsonHandler]
  public abstract class Datum : AmorphousTypedDoc, Log.IArchiveLoggable
  {
    #region CONST
    public const string UNSPECIFIED_SOURCE = "*";
    #endregion

    /// <summary>
    /// True if a The string null/blank or *
    /// </summary>
    public static bool IsUnspecifiedSourceString(string src) => src.IsNullOrWhiteSpace() || src.EqualsOrdSenseCase(UNSPECIFIED_SOURCE);


    #region .ctor
    protected Datum()
    {
      m_StartUtc = Ambient.UTCNow;
    }

    protected Datum(string source)
    {
      m_Source = source;
      m_StartUtc = Ambient.UTCNow;
    }

    protected Datum(string source, DateTime utcDateTime)
    {
      m_Source = source;
      m_StartUtc = utcDateTime;
    }
    #endregion

    #region Fields
    private GDID m_Gdid;
    private string m_Source;
    private long m_Count;
    private DateTime m_StartUtc;
    private DateTime m_EndUtc;
    #endregion

    #region Properties

    /// <summary>
    /// Global distributed ID used by distributed log warehouses. GDID.ZERO for local logging applications
    /// </summary>
    [Field, Field(isArow: true, backendName: "gdid")]
    public GDID Gdid
    {
      get => m_Gdid;
      set => m_Gdid = value;
    }

    /// <summary>
    /// Returns UTC time stamp when event happened
    /// </summary>
    [Field, Field(isArow: true, backendName: "sts")]
    public DateTime StartUtc
    {
      get => m_StartUtc;
      protected set => m_StartUtc = value;
    }

    /// <summary>
    /// Returns UTC time stamp when event happened. This property may be gotten only if IsAggregated==true, otherwise UTCTime value is returned
    /// </summary>
    [Field, Field(isArow: true, backendName: "ets")]
    public DateTime EndUtc
    {
      get => m_Count == 0 ? m_StartUtc : m_EndUtc;
      protected set => m_EndUtc = value;
    }


    /// <summary>
    /// Indicates whether this instance represents a rollup/aggregation of multiple events
    /// </summary>
    public bool IsAggregated => m_Count > 0;

    /// <summary>
    /// Returns count of measurements. This property may be gotten only if IsAggregated==true, otherwise zero is returned
    /// </summary>
    [Field, Field(isArow: true, backendName: "cnt")]
    public long Count
    {
      get => m_Count;
      protected set => m_Count = value;
    }

    /// <summary>
    /// Returns datum source. Data are rolled-up by type of recorded datum instances and source
    /// </summary>
    [Field, Field(isArow: true, backendName: "src")]
    public virtual string Source
    {
      get  => m_Source ?? UNSPECIFIED_SOURCE;
      protected set => m_Source = value;
    }

    /// <summary>
    /// Returns archive dimensions vector which by default short-circuits to Source
    /// </summary>
    string Log.IArchiveLoggable.ArchiveDimensions => Source;

    /// <summary>
    /// True if this instance represent an unspecified source
    /// </summary>
    public bool IsUnspecifiedSource => IsUnspecifiedSourceString(Source);

    /// <summary>
    /// Returns rate of occurrence string
    /// </summary>
    public string Rate
    {
      get
      {
        if (m_Count <= 0) return CoreConsts.UNKNOWN;

        var span = m_EndUtc - m_StartUtc;

        if (m_Count == 1 && span.TotalMilliseconds < 0.1) return string.Empty;

        var rate = m_Count / span.TotalSeconds;

        return Math.Abs(rate) > 1.0 ? "{0:0.00}/sec.".Args(rate) : "{0:0.00}/msec.".Args(1000 * rate);
      }
    }


    /// <summary>
    /// Returns description for data that this datum represents. Base implementation returns full type name of this instance
    /// </summary>
    public virtual string Description => GetType().FullName;


    /// <summary>
    /// Provides access to value polymorphically
    /// </summary>
    public abstract object ValueAsObject { get; }

    /// <summary>
    /// Provides numeric value used for charts
    /// </summary>
    public virtual object PlotValue => ValueAsObject;

    /// <summary>
    /// Provides name for units that value is measured in
    /// </summary>
    public abstract string ValueUnitName { get; }
    #endregion

    #region Public
    private static ConstrainedSetLookup<Type, IEnumerable<Type>> s_ViewGroupInterfaces = new ConstrainedSetLookup<Type, IEnumerable<Type>>( tp =>{
      var result = tp.GetInterfaces()
                     .Where(i => Attribute.IsDefined(i, typeof(InstrumentViewGroup)))
                     .ToArray();
      return result;
    });

    /// <summary>
    /// Returns Datum classification interfaces marked with InstrumentViewGroup attribute. The implementation is cached for efficiency
    /// </summary>
    public IEnumerable<Type> ViewGroupInterfaces => GetViewGroupInterfaces(GetType());

    /// <summary>
    /// Returns Datum classification interfaces marked with InstrumentViewGroup attribute. The implementation is cached for efficiency
    /// </summary>
    public static IEnumerable<Type> GetViewGroupInterfaces(Type tDatum)
      => s_ViewGroupInterfaces[tDatum.IsOfType(typeof(Datum))];

    /// <summary>
    /// Aggregates multiple data instances (e.g.from multiple threads) into one single instance. This is the "reduce" operation which
    /// makes aggregate instance, then concatenates all data events, them finalizes operation by calling SummarizeAggregation()
    /// </summary>
    public Datum Aggregate(IEnumerable<Datum> many)
    {
      var start = DateTime.MaxValue;
      var end = DateTime.MinValue;
      var cnt = 0;

      var result = MakeAggregateInstance();

      foreach (var e in many)
      {
        cnt++;
        if (e.StartUtc < start) start = e.StartUtc;
        if (e.StartUtc > end) end = e.StartUtc;
        result.AggregateEvent(e);
      }

      result.m_Count = cnt;
      result.m_StartUtc = start;
      result.m_EndUtc = end;
      result.SummarizeAggregation();

      return result;
    }


    /// <summary>
    /// Override to set a new source value which is less-specific than existing source.
    /// ReductionLevel specifies how much detail should be lost. The function is idempotent, that is - calling more than once with the same arg does not
    /// change the state of the object.
    /// The default implementation removes all source details (unspecified source) when reductionLevel less than zero.
    /// Example:
    ///  TotalBytesSent("mpx://45.12.123.19:7823 -> MySystem.Contracts.IDoSomething.SomeMethod1()")
    ///  ReduceSourceDetail(0) -> yields original string
    ///  ReduceSourceDetail(1) - > "MySystem.Contracts.IDoSomething.SomeMethod1()"
    ///  ReduceSourceDetail(2) - > "MySystem.Contracts.IDoSomething"
    ///  ReduceSourceDetail(3) - > ""
    /// </summary>
    public virtual void ReduceSourceDetail(int reductionLevel)
    {
      if (reductionLevel < 0) m_Source = null;
    }

    public override string ToString()
    {
      var t = GetType().FullName;
      var d = Description;
      if (t != d) t = t + " '" + d + "'";
      var r = Rate;
      if (r.IsNotNullOrWhiteSpace())
        return "[{0}] {1} {2} @ {3} Value: {4} {5}".Args(t, Source, Count, r, ValueAsObject, ValueUnitName);
      else
        return "[{0}] {1} {2} Value: {3} {4}".Args(t, Source, Count, ValueAsObject, ValueUnitName);
    }
    #endregion

    #region Protected
    protected abstract Datum MakeAggregateInstance();
    protected virtual void AggregateEvent(Datum dat) { }
    protected virtual void SummarizeAggregation() { }


    protected override void AddJsonSerializerField(Schema.FieldDef def, JsonWritingOptions options, Dictionary<string, object> jsonMap, string name, object value)
    {
      if (def.Name==nameof(GDID))
      {
        Serialization.Bix.BixJsonHandler.EmitJsonBixDiscriminator(this, jsonMap);
      }

      base.AddJsonSerializerField(def, options, jsonMap, name, value);
    }

    #endregion
  }
}
