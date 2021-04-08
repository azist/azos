/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Data;
using Azos.Instrumentation;

namespace Azos.Sky.Mdb
{

  public abstract partial class MdbArea
  {
    /// <summary>
    /// Represents partition within the area
    /// </summary>
    public sealed class Partition : MdbAppComponent, IComparable, IEquatable<Partition>, IComparable<Partition>
    {

              /// <summary>
              /// Denotes connection types Primary/Secondary
              /// </summary>
              public enum ShardBackendConnection{Primary=0, Secondary}

              /// <summary>
              /// Represents a SHARD information for the DB particular host
              /// </summary>
              public sealed class Shard : MdbAppComponent, IComparable, IComparable<Shard>, IEquatable<Shard>
              {
                internal Shard(Partition part, IConfigSectionNode config) : base(part)
                {
                  m_Partition = part;
                  m_Order = config.AttrByName(CONFIG_ORDER_ATTR).ValueAsInt(0);

                  PrimaryHostConnectString = ConfigStringBuilder.Build(config, CONFIG_PRIMARY_CONNECT_STRING_ATTR);
                  SecondaryHostConnectString = ConfigStringBuilder.Build(config, CONFIG_SECONDARY_CONNECT_STRING_ATTR);

                  if (PrimaryHostConnectString.IsNullOrWhiteSpace())
                    throw new MdbException(StringConsts.MDB_AREA_CONFIG_SHARD_CSTR_ERROR.Args(part.Area.Name, part.Order, CONFIG_PRIMARY_CONNECT_STRING_ATTR));

                  if (SecondaryHostConnectString.IsNullOrWhiteSpace())
                    throw new MdbException(StringConsts.MDB_AREA_CONFIG_SHARD_CSTR_ERROR.Args(part.Area.Name, part.Order, CONFIG_SECONDARY_CONNECT_STRING_ATTR));

                }

                private Partition m_Partition;
                private int m_Order;

                private ShardBackendConnection  m_ConnectionType;


                public MdbArea Area { get{ return m_Partition.Area;}}
                public Partition Partition { get{ return m_Partition;}}
                public int       Order     { get{ return m_Order;}}


                public readonly string PrimaryHostConnectString;
                public readonly string SecondaryHostConnectString;


                /// <summary>
                /// Returns Primary then secondary connect strings
                /// </summary>
                public IEnumerable<string> ConnectStrings
                {
                  get
                  {
                    yield return PrimaryHostConnectString;
                    yield return SecondaryHostConnectString;
                  }
                }


                /// <summary>
                /// Returns either primary or secondary connect string
                /// depending on connection type
                /// </summary>
                [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
                public ShardBackendConnection ConnectionType
                {
                  get
                  {
                    return m_ConnectionType;
                  }
                  set
                  {
                    if (m_ConnectionType!=value)
                    {
                      m_ConnectionType = value;
                      //todo Instrument
                    }
                  }
                }

                /// <summary>
                /// Returns either primary or secondary connect string
                /// depending on connection type
                /// </summary>
                public string EffectiveConnectionString
                {
                  get
                  {
                    return m_ConnectionType==ShardBackendConnection.Primary ?
                           PrimaryHostConnectString : SecondaryHostConnectString;
                  }
                }

                /// <summary>
                /// Returns new wrapper for operations on this shard
                /// </summary>
                public CRUDOperations CRUDOperations
                {
                  get{ return new CRUDOperations(this);}
                }


                public override bool Equals(object obj)
                {
                  var other = obj as Shard;
                  return this.Equals(other);
                }

                public bool Equals(Shard other)
                {
                  if (other==null) return false;
                  return object.ReferenceEquals(this.m_Partition, other.m_Partition) &&
                         this.m_Order == other.m_Order;
                }

                public override int GetHashCode()
                {
                  return m_Order.GetHashCode();
                }

                public override string ToString()
                {
                  return "{0}->[{1}]".Args(m_Partition, m_Order);
                }

                public int CompareTo(object obj)
                {
                  var other = obj as Shard;
                  return this.CompareTo(other);
                }

                public int CompareTo(Shard other)
                {
                  if (other==null) return +1;
                  if (!object.ReferenceEquals(this.m_Partition, other.m_Partition)) return +1;
                  return this.m_Order.CompareTo(other.Order);
                }
              }


      internal Partition(MdbArea area, IConfigSectionNode config) : base(area)
      {
        m_Area = area;

        if (area is MDBCentralArea)
        {
          m_StartGDID = new GDID(0, 0, 0);
        }
        else
        {
          var sgdid = config.AttrByName(CONFIG_START_GDID_ATTR).ValueAsString();

          if (!GDID.TryParse(sgdid, out m_StartGDID))
           throw new MdbException(StringConsts.MDB_AREA_CONFIG_PARTITION_GDID_ERROR.Args(area.Name, sgdid, "unparsable, expected 'era:0:counter'"));

          if (m_StartGDID.Authority!=0)
           throw new MdbException(StringConsts.MDB_AREA_CONFIG_PARTITION_GDID_ERROR.Args(area.Name, sgdid, "authority segment must be 0"));
        }
        //Shards
        var shards = new List<Shard>();
        foreach(var snode in config.Children.Where( cn => cn.IsSameName(CONFIG_SHARD_SECTION)))
        {
          var shard = new Shard(this, snode);
          shards.Add( shard );
        }

        if (shards.Count==0)
          throw new MdbException(StringConsts.MDB_AREA_CONFIG_NO_PARTITION_SHARDS_ERROR.Args(area.Name, this.m_StartGDID));

        if (shards.Count != shards.Distinct().Count())
          throw new MdbException(StringConsts.MDB_AREA_CONFIG_DUPLICATE_PARTITION_SHARDS_ERROR.Args(area.Name, this.m_StartGDID));

        shards.Sort();
        m_Shards = shards.ToArray();
      }

      private MdbArea m_Area;
      private int m_Order; internal void __setOrder(int num) { m_Order = num;}
      private GDID m_StartGDID;
      private Shard[] m_Shards;


      public MdbArea Area   { get{ return m_Area;   }}
      public int Order     { get{ return m_Order;   }}
      public GDID StartGDID { get{ return m_StartGDID;}}
      public Shard[] Shards { get{ return m_Shards;   }}



      /// <summary>
      /// Returns CRUDOperations facade connected to the appropriate database server within this named area and partition
      ///  which services the shard computed from sharding id
      /// </summary>
      public CRUDOperations ShardedOperationsFor(object idSharding)
      {
        var shard = GetShardForID(idSharding);
        return new CRUDOperations(shard);
      }

      /// <summary>
      /// Finds appropriate shard for ID. See MDB.ShardingUtils
      /// </summary>
      public Shard GetShardForID(object idSharding)
      {
        ulong subid = new ShardKey(idSharding).GetDistributedStableHash();

        return Shards[ subid % (ulong)Shards.Length ];
      }

      public override bool Equals(object obj)
      {
        var other = obj as Partition;
        return this.Equals(other);
      }

      public bool Equals(Partition other)
      {
        if (other==null) return false;
        return
            object.ReferenceEquals(this.m_Area, other.m_Area) &&
            GDIDRangeComparer.Instance.Compare(this.StartGDID, other.StartGDID)==0;
      }

      public int CompareTo(object obj)
      {
        var other = obj as Partition;
        return this.CompareTo(other);
      }

      public int CompareTo(Partition other)
      {
        if (other==null) return +1;
        if (!object.ReferenceEquals(this.m_Area, other.m_Area)) return +1;
        return GDIDRangeComparer.Instance.Compare(this.StartGDID, other.StartGDID);
      }

      public override int GetHashCode()
      {
        return this.m_StartGDID.GetHashCode();
      }

      public override string ToString()
      {
        return "Area('{0}').Partition('{1}' starting '{2}' shards {3})".Args(m_Area.Name, m_Order, m_StartGDID, m_Shards.Length);
      }



    }//Partition
  }//MdbArea

}
