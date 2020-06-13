/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Instrumentation;
using Azos.Serialization.Arow;
using Azos.Serialization.BSON;

namespace Azos.Data.Access.Cache.Instrumentation
{
  /// <summary>
  /// Provides base for cache long gauges
  /// </summary>
  [Serializable]
  public abstract class CacheLongGauge : LongGauge, ICacheInstrument, IMemoryInstrument
  {
    protected CacheLongGauge(string src, long value) : base(src, value) { }
  }

  /// <summary>
  /// Provides base for cache double gauges
  /// </summary>
  [Serializable]
  public abstract class CacheDoubleGauge : DoubleGauge, ICacheInstrument, IMemoryInstrument
  {
    protected CacheDoubleGauge(string src, double value) : base(src, value) { }
  }

  /// <summary>
  /// Provides record count in the instance
  /// </summary>
  [Serializable]
  [Arow("19E21AFF-CB76-424D-A589-B9D0E2CB8B9D")]
  public class RecordCount : CacheLongGauge
  {
    internal RecordCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "Provides record count in the instance"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_RECORD; } }

    protected override Datum MakeAggregateInstance() { return new RecordCount(this.Source, 0); }
  }


  /// <summary>
  /// Provides page count in the instance
  /// </summary>
  [Serializable]
  [Arow("23C21C02-35EC-45DE-9C2B-B622D9D0F8C1")]
  public class PageCount : CacheLongGauge
  {
    internal PageCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "Provides page count in the instance"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_PAGE; } }

    protected override Datum MakeAggregateInstance() { return new PageCount(this.Source, 0); }
  }


  /// <summary>
  /// Provides the ratio of how many buckets are loaded with pages vs. bucket count
  /// </summary>
  [Serializable]
  [Arow("F2C26547-E9F2-41BE-AEF4-3AA413F1488B")]
  public class BucketPageLoadFactor : CacheDoubleGauge
  {
    internal BucketPageLoadFactor(string src, double value) : base(src, value) { }

    public override string Description { get { return "Provides the ratio of how many buckets are loaded with pages vs. bucket count"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_PAGE_PER_BUCKET; } }

    protected override Datum MakeAggregateInstance() { return new BucketPageLoadFactor(this.Source, 0); }
  }


  /// <summary>
  /// How many times Get() resulted in cache hit
  /// </summary>
  [Serializable]
  [Arow("2B2E066B-9248-4D14-A066-513BDD112927")]
  public class HitCount : CacheLongGauge
  {
    internal HitCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many times Get() resulted in cache hit"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new HitCount(this.Source, 0); }
  }

  /// <summary>
  /// How many times Get() resulted in cache miss
  /// </summary>
  [Serializable]
  [Arow("80E93C87-F5CC-4112-A2FF-DE1879C23E3E")]
  public class MissCount : CacheLongGauge
  {
    internal MissCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many times Get() resulted in cache miss"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new MissCount(this.Source, 0); }
  }

  /// <summary>
  /// How many times factory func was called from GetOrPut()
  /// </summary>
  [Serializable]
  [Arow("9DE7FE81-7DA4-4E0F-AFA2-820E36D7EE36")]
  public class ValueFactoryCount : CacheLongGauge
  {
    internal ValueFactoryCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many times factory func was called from GetOrPut()"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_CALL; } }

    protected override Datum MakeAggregateInstance() { return new ValueFactoryCount(this.Source, 0); }
  }


  /// <summary>
  /// How many times tables were swept
  /// </summary>
  [Serializable]
  [Arow("9E50E9D9-E269-4615-9322-EF6474EFB7F3")]
  public class SweepTableCount : CacheLongGauge
  {
    internal SweepTableCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many times tables were swept"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_SWEEP; } }

    protected override Datum MakeAggregateInstance() { return new SweepTableCount(this.Source, 0); }
  }

  /// <summary>
  /// How many pages swept
  /// </summary>
  [Serializable]
  [Arow("88B2D743-B5C4-4B9A-81AA-D4A1906B464E")]
  public class SweepPageCount : CacheLongGauge
  {
    internal SweepPageCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many pages swept"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_SWEEP; } }

    protected override Datum MakeAggregateInstance() { return new SweepPageCount(this.Source, 0); }
  }

  /// <summary>
  /// How many records removed by sweep
  /// </summary>
  [Serializable]
  [Arow("554B86FE-0DD7-4514-83BE-487AF979CF50")]
  public class SweepRemoveCount : CacheLongGauge
  {
    internal SweepRemoveCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many records removed by sweep"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_RECORD; } }

    protected override Datum MakeAggregateInstance() { return new SweepRemoveCount(this.Source, 0); }
  }

  /// <summary>
  /// How many times Put() was called
  /// </summary>
  [Serializable]
  [Arow("B63F54AD-2FD5-43DF-8F7F-9CA8BF6C52DE")]
  public class PutCount : CacheLongGauge
  {
    internal PutCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many times Put() was called"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new PutCount(this.Source, 0); }
  }

  /// <summary>
  /// How many times a call to Put() resulted in insert
  /// </summary>
  [Serializable]
  [Arow("1060CFAA-6436-4B77-803E-D7FE0F61C36F")]
  public class PutInsertCount : CacheLongGauge
  {
    internal PutInsertCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many times a call to Put() resulted in insert"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new PutInsertCount(this.Source, 0); }
  }

  /// <summary>
  /// How many times a call to Put() resulted in relacement of existing item by key without collision
  /// </summary>
  [Serializable]
  [Arow("2480354B-2F73-4C2A-80D7-28AE300C5BA1")]
  public class PutReplaceCount : CacheLongGauge
  {
    internal PutReplaceCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many times a call to Put() resulted in relacement of existing item by key without collision"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new PutReplaceCount(this.Source, 0); }
  }

  /// <summary>
  /// How many times a call to Put() resulted in bucket collision that created a page
  /// </summary>
  [Serializable]
  [Arow("C49A46B5-C059-4BFF-B557-BD3E870CCA57")]
  public class PutPageCreateCount : CacheLongGauge
  {
    internal PutPageCreateCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many times a call to Put() resulted in bucket collision that created a page"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_PAGE; } }

    protected override Datum MakeAggregateInstance() { return new PutPageCreateCount(this.Source, 0); }
  }

  /// <summary>
  /// How many times a call to Put() resulted in new value overriding existing because of collision (old value lost)
  /// </summary>
  [Serializable]
  [Arow("C77A0FF1-A8F3-4A07-AAAF-80956FFDEEAF")]
  public class PutCollisionCount : CacheLongGauge
  {
    internal PutCollisionCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many times a call to Put() resulted in new value overriding existing because of collision (old value lost)"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new PutCollisionCount(this.Source, 0); }
  }


  /// <summary>
  /// How many times a call to Put() could have resulted in new value overriding existing one because of collision, however the situation was prevented
  /// because existing item had higher priority than the newer one
  /// </summary>
  [Serializable]
  [Arow("3176B8F3-81CF-4074-9B8B-3AA6871B5795")]
  public class PutPriorityPreventedCollisionCount : CacheLongGauge
  {
    internal PutPriorityPreventedCollisionCount(string src, long value) : base(src, value) { }

    public override string Description
    {
      get
      {
        return
    "How many times a call to Put() could have resulted in new value overriding existing one because of collision, " +
    "however the situation was prevented because existing item had higher priority than the newer one";
      }
    }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new PutPriorityPreventedCollisionCount(this.Source, 0); }
  }

  /// <summary>
  /// How many pages have been deleted, a page gets deleted when there are no records stored in it
  /// </summary>
  [Serializable]
  [Arow("DB3717AB-B6F4-4599-B6D8-000A3C47C916")]
  public class RemovePageCount : CacheLongGauge
  {
    internal RemovePageCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many pages have been deleted, a page gets deleted when there are no records stored in it"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_PAGE; } }

    protected override Datum MakeAggregateInstance() { return new RemovePageCount(this.Source, 0); }
  }

  /// <summary>
  /// How many records have been found and removed
  /// </summary>
  [Serializable]
  [Arow("C94675F9-6984-44BB-A83E-E0DBFFD0243B")]
  public class RemoveHitCount : CacheLongGauge
  {
    internal RemoveHitCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many records have been found and removed"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_RECORD; } }

    protected override Datum MakeAggregateInstance() { return new RemoveHitCount(this.Source, 0); }
  }

  /// <summary>
  /// How many records have been sought to be removed but were not found
  /// </summary>
  [Serializable]
  [Arow("F595768C-9276-4169-A4EC-E046C664FB15")]
  public class RemoveMissCount : CacheLongGauge
  {
    internal RemoveMissCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "How many records have been sought to be removed but were not found"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new RemoveMissCount(this.Source, 0); }
  }
}
