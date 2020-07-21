/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Instrumentation;
using Azos.Serialization.Arow;
using Azos.Serialization.BSON;

namespace Azos.Wave.Cms.Instrumentation
{
  /// <summary>
  /// How many Cms requests made
  /// </summary>
  [Serializable]
  [Arow("ABA005FA-3BD4-4AFA-B62C-92E06DD0B82F")]
  public sealed class RequestCountGauge : LongGauge
  {
    //.ctor used by SnapshotAllLongInto()
    public RequestCountGauge(string src, long val) : base(src, val) { }

    public override string Description => "How many Cms requests made";
    public override string ValueUnitName => CoreConsts.UNIT_NAME_REQUEST;

    protected override Datum MakeAggregateInstance()
     => new RequestCountGauge(Source, 0);
  }

  /// <summary>
  /// How many Cms requests resulted in a found content response
  /// </summary>
  [Serializable]
  [Arow("8CE2816E-9BF5-4C24-AF35-96659957322E")]
  public sealed class ResponseCountGauge : LongGauge
  {
    //.ctor used by SnapshotAllLongInto()
    public ResponseCountGauge(string src, long val) : base(src, val) { }

    public override string Description => "How many Cms requests resulted in a found content response";
    public override string ValueUnitName => CoreConsts.UNIT_NAME_RESPONSE;

    protected override Datum MakeAggregateInstance()
     => new ResponseCountGauge(Source, 0);
  }


  /// <summary>
  /// Cached content record count
  /// </summary>
  [Serializable]
  [Arow("5C0E227C-C5D3-4CC1-9CC8-A9227D747425")]
  public sealed class CacheRecordCountGauge : LongGauge
  {
    //.ctor used by SnapshotAllLongInto()
    public CacheRecordCountGauge(string src, long val) : base(src, val) { }

    public override string Description => "Cached content record count";
    public override string ValueUnitName => CoreConsts.UNIT_NAME_RECORD;

    protected override Datum MakeAggregateInstance()
     => new CacheRecordCountGauge(Source, 0);
  }

  /// <summary>
  /// Expired cache content record count
  /// </summary>
  [Serializable]
  [Arow("A830C246-B30A-4C04-BAF6-DB013748EE0F")]
  public sealed class CacheExpiredRecordCountGauge : LongGauge
  {
    //.ctor used by SnapshotAllLongInto()
    public CacheExpiredRecordCountGauge(string src, long val) : base(src, val) { }

    public override string Description => "Expired cache content record count";
    public override string ValueUnitName => CoreConsts.UNIT_NAME_RECORD;

    protected override Datum MakeAggregateInstance()
     => new CacheExpiredRecordCountGauge(Source, 0);
  }

  /// <summary>
  /// How many times the cache was hit
  /// </summary>
  [Serializable]
  [Arow("C7982DC1-161F-411A-B229-5A5F3D5579C3")]
  public sealed class CacheHitCountGauge : LongGauge
  {
    //.ctor used by SnapshotAllLongInto()
    public CacheHitCountGauge(string src, long val) : base(src, val) { }

    public override string Description => "How many times the content cache was hit";
    public override string ValueUnitName => CoreConsts.UNIT_NAME_TIME;

    protected override Datum MakeAggregateInstance()
     => new CacheHitCountGauge(Source, 0);
  }

  /// <summary>
  /// How many times the cache was hit
  /// </summary>
  [Serializable]
  [Arow("F0B27F69-E6C5-4770-9A48-2900B1E7F353")]
  public sealed class CacheMissCountGauge : LongGauge
  {
    //.ctor used by SnapshotAllLongInto()
    public CacheMissCountGauge(string src, long val) : base(src, val) { }

    public override string Description => "How many times the content cache was hit";
    public override string ValueUnitName => CoreConsts.UNIT_NAME_TIME;

    protected override Datum MakeAggregateInstance()
     => new CacheMissCountGauge(Source, 0);
  }

  /// <summary>
  /// Average content fetch latency
  /// </summary>
  [Serializable]
  [Arow("63B04E33-4C1F-4C2E-855E-E0329E45B3E8")]
  public sealed class ContentFetchLatencyGauge : LongGauge
  {
    //.ctor used by SnapshotAllLongInto()
    public ContentFetchLatencyGauge(string src, long val) : base(src, val) { }

    public override string Description => "Average content fetch latency";
    public override string ValueUnitName => CoreConsts.UNIT_NAME_MSEC;

    protected override Datum MakeAggregateInstance()
     => new ContentFetchLatencyGauge(Source, 0);
  }
}
