/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Instrumentation;
using Azos.Serialization.BSON;

namespace Azos.Sky.Kdb.Instrumentation
{
  /// <summary>
  /// Provides base for KDB long gauges
  /// </summary>
  [Serializable]
  public abstract class KdbLongGauge : LongGauge, IDatabaseInstrument
  {
    protected KdbLongGauge(string src, long value) : base(src, value) { }
  }

  /// <summary>
  /// Provides Get hit count per table
  /// </summary>
  [Serializable]
  [BSONSerializable("F60CD643-CC90-4590-A816-A382F06C41E9")]
  public class GetHitCount : KdbLongGauge
  {
    public GetHitCount(string tbl, long value) : base(tbl, value) { }

    public override string Description { get { return "Provides Get hit count per table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new GetHitCount(this.Source, 0); }
  }

  /// <summary>
  /// Provides Get hit from fallback set count per table
  /// </summary>
  [Serializable]
  [BSONSerializable("2B5EB443-412B-4E5B-94C3-04A5F96A8EC1")]
  public class GetFallbackHitCount : KdbLongGauge
  {
    public GetFallbackHitCount(string tbl, long value) : base(tbl, value) { }

    public override string Description { get { return "Provides Get hit from fallback set count per table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new GetFallbackHitCount(this.Source, 0); }
  }

  /// <summary>
  /// Provides Get miss count per table
  /// </summary>
  [Serializable]
  [BSONSerializable("97D5ACC4-CE8F-4ABD-851A-5546F101D982")]
  public class GetMissCount : KdbLongGauge
  {
    public GetMissCount(string tbl, long value) : base(tbl, value) { }

    public override string Description { get { return "Provides Get miss count per table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new GetMissCount(this.Source, 0); }
  }

  /// <summary>
  /// Provides Get resulted in last use stamp update count per table
  /// </summary>
  [Serializable]
  [BSONSerializable("B636682D-EAA8-4C3F-B886-14F6560AE467")]
  public class GetTouchCount : KdbLongGauge
  {
    public GetTouchCount(string tbl, long value) : base(tbl, value) { }

    public override string Description { get { return "Provides Get resulted in last use stamp update count per table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new GetTouchCount(this.Source, 0); }
  }

  /// <summary>
  /// Provides put count per table
  /// </summary>
  [Serializable]
  [BSONSerializable("0E47D988-22C6-438B-8451-6C23ED681D30")]
  public class PutCount : KdbLongGauge
  {
    public PutCount(string tbl, long value) : base(tbl, value) { }

    public override string Description { get { return "Provides put count per table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new PutCount(this.Source, 0); }
  }

  /// <summary>
  /// Provides delete hit count per table
  /// </summary>
  [Serializable]
  [BSONSerializable("7E4CE481-3AFF-44F2-9364-5AC59E5E0612")]
  public class DeleteHitCount : KdbLongGauge
  {
    public DeleteHitCount(string tbl, long value) : base(tbl, value) { }

    public override string Description { get { return "Provides delete hit count per table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new DeleteHitCount(this.Source, 0); }
  }

  /// <summary>
  /// Provides delete fallback count per table
  /// </summary>
  [Serializable]
  [BSONSerializable("DA220322-90A8-4BDA-89FF-991C5890286E")]
  public class DeleteFallbackCount : KdbLongGauge
  {
    public DeleteFallbackCount(string tbl, long value) : base(tbl, value) { }

    public override string Description { get { return "Provides delete fallback count per table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new DeleteFallbackCount(this.Source, 0); }
  }

  /// <summary>
  /// Provides delete miss count per table
  /// </summary>
  [Serializable]
  [BSONSerializable("EB587F38-1F5E-4A72-B7A7-2D38B4232F75")]
  public class DeleteMissCount : KdbLongGauge
  {
    public DeleteMissCount(string tbl, long value) : base(tbl, value) { }

    public override string Description { get { return "Provides delete miss count per table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new DeleteMissCount(this.Source, 0); }
  }

  /// <summary>
  /// Provides error count per table
  /// </summary>
  [Serializable]
  [BSONSerializable("B01A6C89-B9E4-47DE-ACC7-AC8BC4A6163D")]
  public class ErrorCount : KdbLongGauge, IErrorInstrument
  {
    public ErrorCount(string tbl, long value) : base(tbl, value) { }

    public override string Description { get { return "Provides error count per table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new ErrorCount(this.Source, 0); }
  }

  /// <summary>
  /// Provides number of records expired with expiration days per table
  /// </summary>
  [Serializable]
  [BSONSerializable("DA2E5293-8ED0-49B8-B578-47F9B9D13A3A")]
  public class SlidingExpirationCount : KdbLongGauge
  {
    public SlidingExpirationCount(string tbl, long value) : base(tbl, value) { }

    public override string Description { get { return "Provides number of records expired with expiration days per table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_RECORD; } }

    protected override Datum MakeAggregateInstance() { return new SlidingExpirationCount(this.Source, 0); }
  }

  /// <summary>
  /// Provides number of records expired with absolute expiration per table
  /// </summary>
  [Serializable]
  [BSONSerializable("CC6B3137-9783-40D3-B269-18CD15B8DA0A")]
  public class AbsoluteExpirationCount : KdbLongGauge
  {
    public AbsoluteExpirationCount(string tbl, long value) : base(tbl, value) { }

    public override string Description { get { return "Provides number of records expired with absolute expiration per table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_RECORD; } }

    protected override Datum MakeAggregateInstance() { return new AbsoluteExpirationCount(this.Source, 0); }
  }

  /// <summary>
  /// Provides number of records moved from fallback to current shard set per table
  /// </summary>
  [Serializable]
  [BSONSerializable("E0FBB4E8-C74C-4185-A981-2FC3CA3E5607")]
  public class MigrationCount : KdbLongGauge
  {
    public MigrationCount(string tbl, long value) : base(tbl, value) { }

    public override string Description { get { return "Provides number of records moved from fallback to current shard set per table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_RECORD; } }

    protected override Datum MakeAggregateInstance() { return new MigrationCount(this.Source, 0); }
  }
}
