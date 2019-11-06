/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Instrumentation;

namespace Azos.Wave.Cms.Instrumentation
{
  /// <summary>
  /// How many Cms requests made
  /// </summary>
  [Serializable]
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
