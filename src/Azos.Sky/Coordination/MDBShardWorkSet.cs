using System.Collections.Generic;
using System.Linq;

using Azos.Sky.MDB;

namespace Azos.Sky.Coordination
{
  /// <summary>
  /// Coordinates work on multiple shards in the MDB area
  /// </summary>
  public class MDBShardWorkSet : WorkSet<MDBArea.Partition.Shard>
  {
    public MDBShardWorkSet(MDBArea area, string name = null)
      : this(null, area, name)
    { }

    public MDBShardWorkSet(string path, MDBArea area, string name = null) : base(path, "{0}.{1}".Args(area.NonNull(text: "area==null)").Name, name))
    {
      Area = area;
      Touch();
    }

    public readonly MDBArea Area;

    protected override void AssignWorkSegment()
    {
      m_TotalWorkCount = Area.AllShards.Count();
      m_MyWorkCount = TaskUtils.AssignWorkSegment(m_TotalWorkCount, WorkerCount, MyIndex, out m_MyFirstWorkIndex);
    }

    protected override IEnumerator<MDBArea.Partition.Shard> GetSegmentEnumerator()
    {
      if (m_MyFirstWorkIndex<0 || m_MyWorkCount<=0)
        return Enumerable.Empty<MDBArea.Partition.Shard>().GetEnumerator();

      return Area.AllShards
                  .Skip(m_MyFirstWorkIndex)
                  .Take(m_MyWorkCount)
                  .GetEnumerator();
    }
  }

}
