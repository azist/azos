/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Data;

namespace Azos.Sky.Mdb
{
  /// <summary>
  /// Represents a non-central area with partitions
  /// </summary>
  public sealed class MdbPartitionedArea : MdbArea
  {

    internal MdbPartitionedArea(MdbDataStore store, IConfigSectionNode node) : base(store, node)
    {
      m_Name = node.AttrByName(Configuration.CONFIG_NAME_ATTR).ValueAsString(string.Empty).Trim();

      if (m_Name.IsNullOrWhiteSpace())
       throw new MdbException(StringConsts.MDB_AREA_CONFIG_NO_NAME_ERROR);

      if (m_Name.EqualsOrdIgnoreCase(CENTRAL_AREA_NAME))
       throw new MdbException(StringConsts.MDB_AREA_CONFIG_INVALID_NAME_ERROR+"MDBPartitionedArea can not be called '{0}'".Args(CENTRAL_AREA_NAME));

      m_Partitions = new List<Partition>();
      foreach(var pnode in node.Children.Where( cn => cn.IsSameName(CONFIG_PARTITION_SECTION)))
      {
        var partition = new Partition(this, pnode);
        m_Partitions.Add( partition );
      }

      if (m_Partitions.Count==0)
       throw new MdbException(StringConsts.MDB_AREA_CONFIG_NO_PARTITIONS_ERROR.Args(Name));

      if (m_Partitions.Count != m_Partitions.Distinct().Count())
       throw new MdbException(StringConsts.MDB_AREA_CONFIG_DUPLICATE_RANGES_ERROR.Args(Name));

      m_Partitions.Sort();// sort by StartGDID

      for(var i=0; i<m_Partitions.Count; i++)
       m_Partitions[i].__setOrder(i);

    }

    private string m_Name;

    private List<Partition> m_Partitions;

    public override string Name{ get{ return m_Name;}}


    /// <summary>
    /// Returns partitions ordered by StartGDID
    /// </summary>
    public IEnumerable<Partition> Partitions{ get{ return m_Partitions;}}

    public override IEnumerable<Partition> AllPartitions { get { return Partitions;} }


    public override IEnumerable<Partition.Shard> AllShards
    {
      get
      {
        foreach(var p in AllPartitions)
         foreach(var s in p.Shards)
          yield return s;
      }
    }

    /// <summary>
    /// Returns CRUDOperations facade connected to the appropriate database server within this named area
    ///  which services the shard computed from briefcase GDID
    /// </summary>
    public CRUDOperations PartitionedOperationsFor(GDID idBriefcase)
    {
      var partition = FindPartitionForBriefcase(idBriefcase);
      return partition.ShardedOperationsFor(new ShardKey(idBriefcase));
    }


    /// <summary>
    /// Returns a partition that holds the data for a briefcase identified by id
    /// </summary>
    public Partition FindPartitionForBriefcase(GDID idBriefcase)
    {
       Partition partition = null;
       for(var i=0; i<m_Partitions.Count; i++)
       {
         var current = m_Partitions[i];
         if (GDIDRangeComparer.Instance.Compare(current.StartGDID, idBriefcase)>0) break;
         partition = current;
       }

       if (partition==null)
         throw new MdbException(StringConsts.MDB_AREA_CONFIG_PARTITION_NOT_FOUND_ERROR.Args(Name, idBriefcase));

       return partition;
    }

    /// <summary>
    /// Returns an enumerations partitions that start from the specified briefcase identified by id.
    /// If briefcase is null or zero then all partitions are returned
    /// </summary>
    public IEnumerable<Partition> GetPartitionsStartingFromBriefcase(GDID? idBriefcase)
    {
      if (!idBriefcase.HasValue || idBriefcase.Value.IsZero) return m_Partitions;
      var partition = FindPartitionForBriefcase(idBriefcase.Value);
      return m_Partitions.Skip(partition.Order);
    }

    public override string ToString()
    {
      return "Area('{0}')[{1}]".Args(this.Name, m_Partitions.Count);
    }

  }

}
