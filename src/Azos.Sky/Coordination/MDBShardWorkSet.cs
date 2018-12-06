using System.Collections.Generic;
using System.Linq;

using Azos.Sky.Mdb;

namespace Azos.Sky.Coordination
{
  /// <summary>
  /// Coordinates work on multiple shards in the MDB area
  /// </summary>
  public class MDBShardWorkSet : WorkSet<MdbArea.Partition.Shard>
  {
    public MDBShardWorkSet(MdbArea area, string name = null)
      : this(null, area, name)
    { }

    public MDBShardWorkSet(string path, MdbArea area, string name = null) : base(path, "{0}.{1}".Args(area.NonNull(nameof(area)).Name, name))
    {
      Area = area;
      Touch();
    }

    public readonly MdbArea Area;

    public override string ComponentLogTopic => SysConsts.LOG_TOPIC_MDB;

    protected override void AssignWorkSegment()
    {
      m_TotalWorkCount = Area.AllShards.Count();
      m_MyWorkCount = TaskUtils.AssignWorkSegment(m_TotalWorkCount, WorkerCount, MyIndex, out m_MyFirstWorkIndex);
    }

    protected override IEnumerator<MdbArea.Partition.Shard> GetSegmentEnumerator()
    {
      if (m_MyFirstWorkIndex<0 || m_MyWorkCount<=0)
        return Enumerable.Empty<MdbArea.Partition.Shard>().GetEnumerator();

      return Area.AllShards
                 .Skip(m_MyFirstWorkIndex)
                 .Take(m_MyWorkCount)
                 .GetEnumerator();
    }
  }

}
