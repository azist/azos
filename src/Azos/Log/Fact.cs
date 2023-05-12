/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Data;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Log
{
  /// <summary>
  /// Marker interface for extension methods: IEnumerable(T: Fact)
  /// </summary>
  public interface IFacts<T> : IEnumerable<T> where T : Fact
  {
  }

  /// <summary>
  /// A uniform model for facts recorded by various systems for the purpose of deferred analytical processing
  /// (such as big data/ML/statistical analysis). Facts are typically encapsulated in log message archives as structured data.
  /// Analysis algorithms use this model as a data source <see cref="IFacts{T}"/>.
  /// You can define your own sub-type to extract Dimensions/Metrics into custom typed properties in descendants
  /// overriding <see cref="DoAmorphousDataAfterLoad(string)"/>, however fact do NOT preserve type identity on
  /// serialization/deserialization by design, the extension pattern with custom sub-typing is only beneficial for adding
  /// calculated fields to avoid costly dynamic dictionary access in cases of multiple field access
  /// </summary>
  [Bix("abf7481c-d32e-4236-a34e-b65983a75fe0")]
  [Schema(Immutable = true,
          Description = "A uniform model for facts emitted by various systems " +
                        "for the purpose of analytical processing (such as ML/statistical analysis)")]
  public class Fact : AmorphousTypedDoc
  {
    /// <summary> True by default for facts </summary>
    public override bool AmorphousDataEnabled => true;

    /// <summary>
    /// A type of fact (e.g. `volt` for voltage measurement)
    /// </summary>
    [Field(Required = true, Description = "A type of fact (e.g. `volt` for voltage measurement)")]
    public Atom FactType { get; set; }

    /// <summary>
    /// Unique fact identifier
    /// </summary>
    [Field(Required = true, Description = "Unique fact identifier")]
    public Guid Id { get; set; }

    /// <summary>
    /// Optional Guid for co-related facts. If unset, equals to `Guid.Empty`
    /// </summary>
    [Field(Required = true, Description = "Optional Guid for co-related facts. If unset, equals to `Guid.Empty`")]
    public Guid RelatedId { get; set; }

    /// <summary>
    /// Global distributed unique id used by the storage layer (Chronicle)
    /// </summary>
    [Field(Required = true, Description = "Global distributed unique id used by the storage layer (Chronicle)")]
    public GDID Gdid { get; set; }

    /// <summary>
    /// System channel used for transmission/storage
    /// </summary>
    [Field(Required = true, Description = "System channel used for transmission/storage")]
    public Atom Channel { get; set; }

    /// <summary>
    /// System fact topic which can be used as a namespace to organize facts into distinct groups
    /// </summary>
    [Field(Required = false, Description = "System fact topic which can be used as a namespace to organize facts into distinct groups")]
    public Atom Topic { get; set; }

    /// <summary>
    /// System host which recorded the fact
    /// </summary>
    [Field(Required = false, Description = "System host which recorded the fact")]
    public string Host { get; set; }

    /// <summary>
    /// System application which recorded the fact
    /// </summary>
    [Field(Required = true, Description = "System application which recorded the fact")]
    public Atom App { get; set; }

    /// <summary>
    /// Type of fact message record, such as Info/Warning/Error/Trace
    /// </summary>
    [Field(Required = true, Description = "Type of fact message record, such as Info/Warning/Error/Trace")]
    public Azos.Log.MessageType RecordType { get; set; }

    /// <summary>
    /// Fact recorder tracepoint/source id (such as line number, or submodule id)
    /// </summary>
    [Field(Required = false, Description = "Fact recorder tracepoint/source id (such as line number, or submodule id)")]
    public int Source { get; set; }

    /// <summary>
    /// Timestamp when the fact was recorded
    /// </summary>
    [Field(Required = true, Description = "Timestamp when the fact was recorded")]
    public DateTime UtcTimestamp { get; set; }

    /// <summary>
    /// Fact dimensions describe `key` dimension attributes which define where/how the metrics vector was obtained
    /// </summary>
    [Field(Required = false, Description = "Fact dimensions describe `key` dimension attributes which define where/how the metrics vector was obtained")]
    public JsonDataMap Dimensions { get; set; }

    /// <summary>
    /// Fact metrics - event data vector stores measured/recorded factual data
    /// </summary>
    [Field(Required = false, Description = "Fact metrics - event data vector stores measured/recorded factual data")]
    public JsonDataMap Metrics { get; set; }

    /// <summary>
    /// Override this method to populate custom derivative state from dynamic <see cref="Dimensions"/> and/or <see cref="Metrics"/>
    /// into your own properties which you can declare in a descendant type implementation.
    /// This makes sense in scenarios when tight performance is needed or when some data pieces/state
    /// need to be pre-calculated once and then reused down the functional chain.
    /// This method may be called more then once so it has to be logically idempotent
    /// </summary>
    protected override void DoAmorphousDataAfterLoad(string targetName)
    {

    }
  }

}
