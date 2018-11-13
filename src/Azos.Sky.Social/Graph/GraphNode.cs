using System;

using Azos.Data;

using Azos.Sky.Social.Graph.Server.Data;

namespace Azos.Sky.Social.Graph
{
  /// <summary>
  /// Contains data about the node of the social graph
  /// </summary>
  [Serializable]
  public struct GraphNode
  {

    /// <summary>
    /// Creates a new GraphNode with new GDID
    /// </summary>
    public static GraphNode MakeNew(string nodeType, GDID gOrigShard, GDID gOrig, string origName, byte[] origData, FriendVisibility defaultFriendVisibility)
    {
      var gdid = NodeRow.GenerateNewNodeRowGDID();
      var utcNow  = App.TimeSource.UTCNow;
      return new GraphNode(nodeType, gdid, gOrigShard, gOrig, origName, origData, utcNow, defaultFriendVisibility);
    }

    public static GraphNode Copy(GraphNode from,
                                 string origName,
                                 byte[] originData,
                                 DateTime utcTimestamp,
                                 FriendVisibility defaultFriendVisibility)
    {
      return new GraphNode(from.NodeType, from.GDID, from.G_OriginShard, from.G_Origin, origName ?? from.OriginName, originData ?? from.OriginData, utcTimestamp, from.DefaultFriendVisibility);
    }

    /// <summary>
    /// Creates GraphNode, when GDID is assigned the graph system tries to update existing node by GDID, otherwise
    /// creates a new node with the specified GDID. Use MakeNew() to generate new GDID
    /// </summary>
    public GraphNode(string nodeType,
                     GDID gdid,
                     GDID gOrigShard,
                     GDID gOrig,
                     string origName,
                     byte[] originData,
                     DateTime utcTimestamp,
                     FriendVisibility defaultFriendVisibility)
    {
      if (nodeType.IsNullOrWhiteSpace() || nodeType.Length>Server.Data.Schema.GSNodeType.MAX_LEN)
        throw new GraphException(StringConsts.ARGUMENT_ERROR+"GraphNode.ctor(nodeType=null|>{0})".Args(Server.Data.Schema.GSNodeType.MAX_LEN));

      NodeType = nodeType;
      GDID = gdid;
      G_OriginShard = gOrigShard;
      G_Origin = gOrig;
      OriginName = origName;
      OriginData = originData;
      TimestampUTC = utcTimestamp;
      DefaultFriendVisibility = defaultFriendVisibility;
    }

    /// <summary>
    /// Returns true if this struct is not assigned any value
    /// </summary>
    public bool Unassigned { get{ return NodeType==null;} }

    /// <summary>
    /// This value depends on the origin database types, i.e.: User, Organization, Forum...
    /// </summary>
    public readonly string NodeType;

    /// <summary>
    /// Graph Node GDID, unique through different types.
    /// The GraphSystem makes this GDID and returns back to the origin for stamping, so
    /// original entity may find this GraphNode by this GDID
    /// </summary>
    public readonly GDID     GDID;

    /// <summary>The sharding GDID in the business origin database, (i.e. G_USER - sharding key)</summary>
    public readonly GDID     G_OriginShard;

    /// <summary>
    /// The GDID of the entity of NodeType in the business origin database,
    /// (e.g. G_USERCAR (if the car is kept in user's shard)) - this may or may not be the same as G_OriginShard
    /// </summary>
    public readonly GDID     G_Origin;
    /// <summary>
    /// Description, e.g. for user - screen name
    /// </summary>
    public readonly string   OriginName;
    /// <summary>
    /// Data about node - BSON
    /// </summary>
    public readonly byte[] OriginData;
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public readonly DateTime TimestampUTC;
    /// <summary>
    /// Determines the default setting of who can see friends of this nodes
    /// </summary>
    public readonly FriendVisibility DefaultFriendVisibility;

    public override string ToString()
    {
      return "GN({0}, {1}, '{2}')".Args(GDID, NodeType, OriginName);
    }

  }
}
