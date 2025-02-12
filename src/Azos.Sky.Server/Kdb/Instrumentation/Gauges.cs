/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Instrumentation;
using Azos.Serialization.Bix;
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
  [Bix("AE88CED6-308C-44FB-BB7A-1EFE16AC46A0")]
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
  [Bix("A03F8D4E-8A3F-46D0-987D-C30C14F904ED")]
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
  [Bix("5B4D906C-6AAD-42E9-A5D4-50B437543D74")]
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
  [Bix("D3C515EC-1700-407C-9A30-731D1C54BD77")]
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
  [Bix("8D7F0F5A-8113-4079-B7E3-E00F2D9DC8CB")]
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
  [Bix("A6D1E9ED-189E-4426-AF07-8DDCDADD4075")]
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
  [Bix("593DD67F-40F2-4A6E-81AA-BB0EEB51216F")]
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
  [Bix("F061A1B9-3B54-4CA0-AB7A-872DA6F27B45")]
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
  [Bix("400B265C-90D2-46F6-A346-E67624AC419E")]
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
  [Bix("5BC901E3-D739-4505-B7B0-5F7687BCE3D0")]
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
  [Bix("27C3065E-2B53-4055-9E8C-FB617BDE3E61")]
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
  [Bix("9D7194F0-33AF-46C3-9A37-F13C920E6B0D")]
  public class MigrationCount : KdbLongGauge
  {
    public MigrationCount(string tbl, long value) : base(tbl, value) { }

    public override string Description { get { return "Provides number of records moved from fallback to current shard set per table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_RECORD; } }

    protected override Datum MakeAggregateInstance() { return new MigrationCount(this.Source, 0); }
  }
}
