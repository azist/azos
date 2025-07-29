/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Instrumentation;
using Azos.Serialization.Bix;

namespace Azos.Pile.Instrumentation
{
  /// <summary>
  /// Provides base for pile long gauges
  /// </summary>
  [Serializable]
  public abstract class PileLongGauge : LongGauge, ICacheInstrument, IMemoryInstrument
  {
    protected PileLongGauge(string src, long value) : base(src, value) { }
  }

  /// <summary>
  /// Provides base for pile double gauges
  /// </summary>
  [Serializable]
  public abstract class PileDoubleGauge : DoubleGauge, ICacheInstrument, IMemoryInstrument
  {
    protected PileDoubleGauge(string src, double value) : base(src, value) { }
  }


  /// <summary>
  /// Provides object count in the instance
  /// </summary>
  [Serializable]
  [Bix("1C59B258-578C-4BD2-8B88-5DA52E8E8299")]
  public class ObjectCount : PileLongGauge
  {
    internal ObjectCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "Provides object count in the instance"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_OBJECT; } }

    protected override Datum MakeAggregateInstance() { return new ObjectCount(this.Source, 0); }
  }


  /// <summary>
  /// Provides segment count in the instance
  /// </summary>
  [Serializable]
  [Bix("5F643847-F558-4A0F-8F31-57E3D8101020")]
  public class SegmentCount : PileLongGauge
  {
    internal SegmentCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "Provides segment count in the instance"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_SEGMENT; } }

    protected override Datum MakeAggregateInstance() { return new SegmentCount(this.Source, 0); }
  }

  /// <summary>
  /// Number of bytes allocated by the instance from system memory
  /// </summary>
  [Serializable]
  [Bix("9EFFAD52-6FB3-48D6-BC58-64F9C2D6D034")]
  public class AllocatedMemoryBytes : PileLongGauge
  {
    internal AllocatedMemoryBytes(string src, long value) : base(src, value) { }

    public override string Description { get { return "Bytes allocated by the instance from system memory"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_BYTES; } }

    protected override Datum MakeAggregateInstance() { return new AllocatedMemoryBytes(this.Source, 0); }
  }


  /// <summary>
  /// Average capacity of free memory that the system has left
  /// </summary>
  [Serializable]
  [Bix("7E4E9689-4614-4D74-88C7-1364EE9B2FA5")]
  public class MemoryCapacityBytes : PileLongGauge
  {
    internal MemoryCapacityBytes(string src, long value) : base(src, value) { }

    public override string Description { get { return "Average capacity of free memory that the system has left"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_BYTES; } }

    protected override Datum MakeAggregateInstance() { return new MemoryCapacityBytes(this.Source, 0); }
  }


  /// <summary>
  /// Number of bytes allocated for object storage within AllocatedMemoryBytes
  /// </summary>
  [Serializable]
  [Bix("035E23BA-5604-4419-AAB7-CCD54910FF0D")]
  public class UtilizedBytes : PileLongGauge
  {
    internal UtilizedBytes(string src, long value) : base(src, value) { }

    public override string Description { get { return "Number of bytes allocated for object storage within AllocatedMemoryBytes"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_BYTES; } }

    protected override Datum MakeAggregateInstance() { return new UtilizedBytes(this.Source, 0); }
  }


  /// <summary>
  /// Number of extra bytes used by pile metadata currently occupied by object stored in this pile
  /// </summary>
  [Serializable]
  [Bix("8573CF6E-9A38-40C3-A415-1213067E484B")]
  public class OverheadBytes : PileLongGauge
  {
    internal OverheadBytes(string src, long value) : base(src, value) { }

    public override string Description { get { return "Number of extra bytes used by pile metadata currently occupied by object stored in this pile"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_BYTES; } }

    protected override Datum MakeAggregateInstance() { return new OverheadBytes(this.Source, 0); }
  }


  /// <summary>
  /// Number of bytes for average object
  /// </summary>
  [Serializable]
  [Bix("ABE80333-5A3E-4140-9FB2-384018886FED")]
  public class AverageObjectSizeBytes : PileLongGauge
  {
    internal AverageObjectSizeBytes(string src, long value) : base(src, value) { }

    public override string Description { get { return "Number of bytes for average object"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_BYTES; } }

    protected override Datum MakeAggregateInstance() { return new AverageObjectSizeBytes(this.Source, 0); }
  }


  /// <summary>
  /// Count of Put() calls
  /// </summary>
  [Serializable]
  [Bix("E2769E8E-7617-4B63-8EC1-5FE586CF6947")]
  public class PutCount : PileLongGauge
  {
    internal PutCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "Count of Put() calls"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new PutCount(this.Source, 0); }
  }

  /// <summary>
  /// Count of Delete() calls
  /// </summary>
  [Serializable]
  [Bix("03BB1698-DAD6-4DB8-A656-A00764BF14D6")]
  public class DeleteCount : PileLongGauge
  {
    internal DeleteCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "Count of Delete() calls"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new DeleteCount(this.Source, 0); }
  }


  /// <summary>
  /// Count of Get() calls
  /// </summary>
  [Serializable]
  [Bix("90C0560D-0F2C-44CA-9733-C9A2EC9E1AD0")]
  public class GetCount : PileLongGauge
  {
    internal GetCount(string src, long value) : base(src, value) { }

    public override string Description { get { return "Count of Get() calls"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TIME; } }

    protected override Datum MakeAggregateInstance() { return new GetCount(this.Source, 0); }
  }

  /// <summary>
  /// Number of free clots(chunks) in the free list
  /// </summary>
  [Serializable]
  [Bix("6FA6A485-C720-4DFA-B955-86044D5086DA")]
  public class FreeListCapacity : PileLongGauge
  {
    internal FreeListCapacity(string src, long value) : base(src, value) { }

    public override string Description { get { return "Number of free clots(chunks) in the free list"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_SLOT; } }

    protected override Datum MakeAggregateInstance() { return new FreeListCapacity(this.Source, 0); }
  }
}
