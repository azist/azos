/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Azos.Apps;
using Azos.Conf;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.Instrumentation
{
  /// <summary>
  /// Stipulates instrumentation contract
  /// </summary>
  public interface IInstrumentation : IApplicationComponent, ILocalizedTimeProvider
  {
    /// <summary>
    /// Indicates whether instrumentation is enabled
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    /// Returns true to indicate that instrumentation does not have any space left to record more data at the present moment
    /// </summary>
    bool Overflown { get; }

    /// <summary>
    /// Returns current record count in the instance
    /// </summary>
    int RecordCount { get; }

    /// <summary>
    /// Gets/Sets the maximum record count that this instance can store
    /// </summary>
    int MaxRecordCount { get; }

    /// <summary>
    /// Specifies how often aggregation is performed
    /// </summary>
    int ProcessingIntervalMS { get; }

    /// <summary>
    /// Specifies how often OS instrumentation such as CPU and RAM is sampled.
    /// Value of zero disables OS sampling
    /// </summary>
    int OSInstrumentationIntervalMS { get; }

    /// <summary>
    /// When true, outputs instrumentation data about the self (how many datum buffers, etc.)
    /// </summary>
    bool SelfInstrumented { get; }

    /// <summary>
    /// Returns the size of the ring buffer where result (aggregated) instrumentation records are kept in memory.
    /// The maximum buffer capacity is returned, not how many results have been buffered so far.
    ///  If this property is less than or equal to zero then result buffering in memory is disabled
    /// </summary>
    int ResultBufferSize { get; }

    /// <summary>
    /// Enumerates distinct types of Datum ever recorded in the instance. This property may be used to build
    ///  UIs for instrumentation, i.e. datum type tree. Returned data is NOT ORDERED
    /// </summary>
    IEnumerable<Type> DataTypes { get; }

    /// <summary>
    /// Enumerates sources per Datum type ever recorded by the instance. This property may be used to build
    ///  UIs for instrumentation, i.e. datum type tree. Returned data is NOT ORDERED.
    ///  Returns default instance so caller may get default description/unit name
    /// </summary>
    IEnumerable<HASKey> GetDatumTypeSources(Type datumType, out Datum defaultInstance);

    /// <summary>
    /// Records instrumentation datum
    /// </summary>
    void Record(Datum datum);

    /// <summary>
    /// Returns the specified number of samples from the ring result buffer in the near-chronological order,
    /// meaning that data is already sorted by time MOST of the TIME, however sorting is NOT GUARANTEED for all
    ///  result records returned as enumeration is a lazy procedure that does not make copies/take locks.
    /// The enumeration is empty if ResultBufferSize is less or equal to zero entries.
    /// If count is less or equal to zero then the system returns all results available.
    /// </summary>
    IEnumerable<Datum> GetBufferedResults(int count = 0);

    /// <summary>
    /// Returns samples starting around the specified UTCdate in the near-chronological order,
    /// meaning that data is already sorted by time MOST of the TIME, however sorting is NOT GUARANTEED for all
    ///  result records returned as enumeration is a lazy procedure that does not make copies/take locks.
    /// The enumeration is empty if ResultBufferSize is less or equal to zero entries
    /// </summary>
    IEnumerable<Datum> GetBufferedResultsSince(DateTime utcStart);
  }


  /// <summary>
  /// Stipulates instrumentation contract
  /// </summary>
  public interface IInstrumentationImplementation : IInstrumentation, IDisposable, IConfigurable, IInstrumentable
  {

  }


  /// <summary>
  /// Denotes an entity that can be instrumented
  /// </summary>
  public interface IInstrumentable : IExternallyParameterized
  {
    /// <summary>
    /// Turns on/off instrumentation
    /// </summary>
    bool InstrumentationEnabled { get; set; }
  }

  /// <summary>
  /// Acts as a grouping key, a tuple of (host,app,source)
  /// </summary>
  public struct HASKey : IEquatable<HASKey>, IJsonWritable
  {
    public HASKey(string host, Atom app, string source)
    {
      this.Host = host.Default(Datum.UNSPECIFIED_HOST);
      this.App = app.IsZero ? Datum.UNSPECIFIED_APP : app;
      this.Source = source.Default(Datum.UNSPECIFIED_SOURCE);
    }
    public readonly string Host;
    public readonly Atom App;
    public readonly string Source;

    public bool IsAssigned => Host != null;

    public bool Equals(HASKey other) => this.App == other.App && this.Host.EqualsOrdSenseCase(other.Host) && this.Source.EqualsOrdSenseCase(other.Source);
    public override bool Equals(object obj) => obj is HASKey has ? this.Equals(has) : false;
    public override int GetHashCode() => (Host != null ? Host.GetHashCode() : 0) ^
                                          (Source != null ? Source.GetHashCode() : 0) ^
                                          App.GetHashCode();

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
      => JsonWriter.WriteMap(wri, nestingLevel, options, new DictionaryEntry("h", Host),
                                                         new DictionaryEntry("a", App),
                                                         new DictionaryEntry("s", Source));
  }

}
