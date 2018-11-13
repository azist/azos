using System.Collections.Generic;
using System.Linq;

using Azos.Pile;
using Azos.Data;
using Azos.Data.Access;
using Azos.Conf;

using Azos.Sky.Mdb;
using Azos.Sky.Social.Graph.Server.Data;
using Azos.Sky.Social.Graph.Server.Data.Schema;

namespace Azos.Sky.Social.Graph.Server
{
  public class GraphCommentFetchDefaultStrategy : DisposableObject, IConfigurable
  {

    public GraphCommentFetchDefaultStrategy(GraphSystemService graphSystemService, IConfigSectionNode config)
    {
      m_GraphSystemService = graphSystemService;
      m_ConfigNode = config;
      ConfigAttribute.Apply(this, config);
    }

    protected GraphSystemService m_GraphSystemService;
    protected IConfigSectionNode m_ConfigNode;

    protected GraphHost GraphHost { get { return GraphOperationContext.Instance.GraphHost; } }
    protected IMdbDataStore DataStore { get { return m_GraphSystemService.DataStore; } }
    protected ICache Cache { get { return m_GraphSystemService.Cache; } }
    protected int MaxLastCommentVolumes { get { return GraphSystemService.MAX_SCAN_TAIL_COMMENT_VOLUMES; } }

    public void Configure(IConfigSectionNode node)
    {

    }

    public virtual IEnumerable<Comment> Fetch(CommentQuery query)
    {
      var start = query.BlockIndex * CommentQuery.COMMENT_BLOCK_SIZE;
      var comments = Cache.FetchThrough<CommentQuery, IEnumerable<Comment>>(query,
        SocialConsts.GS_COMMENT_BLOCK_TBL,
        CacheParams.ReadFreshWriteSec(0), // todo TO DISCUSS
        commQry =>
        {
          var rows = new List<CommentRow>();
          var qryVolumes = Queries.FindCommentVolumes<CommentVolumeRow>(query.G_TargetNode, query.Dimension, MaxLastCommentVolumes, query.AsOfDate);
          var volumes = ForNode(query.G_TargetNode).LoadEnumerable(qryVolumes);
          foreach (var volume in volumes)
          {
            var qryComments = Queries.FindComments<CommentRow>(query.G_TargetNode, query.Dimension, true);
            var loadRows = ForComment(volume.G_CommentVolume).LoadEnumerable(qryComments);
            rows.AddRange(loadRows);
          }

          IEnumerable<CommentRow> orderedRows = null;

          switch (query.OrderType)
          {
            case CommentOrderType.ByDate:
              if (query.Ascending)
                orderedRows = rows.OrderBy(row => row.Create_Date);
              else
                orderedRows = rows.OrderByDescending(row => row.Create_Date);
              break;
            case CommentOrderType.ByPositive:
              orderedRows = rows.OrderBy(row => row.Rating)
                .ThenByDescending(row => row.Create_Date);
              break;
            case CommentOrderType.ByNegative:
              orderedRows = rows.OrderByDescending(row => row.Rating)
                .ThenByDescending(row => row.Create_Date);
              break;
            case CommentOrderType.ByPopular:
              if (query.Ascending)
                orderedRows = rows.OrderBy(row => row.Like + row.Dislike)
                  .ThenByDescending(row => row.Create_Date);
              else
                orderedRows = rows.OrderByDescending(row => row.Like + row.Dislike)
                  .ThenByDescending(row => row.Create_Date);
              break;
            case CommentOrderType.ByUsefull:
              if (query.Ascending)
                orderedRows = rows.OrderBy(row => row.Like - row.Dislike)
                  .ThenByDescending(row => row.Create_Date);
              else
                orderedRows = rows.OrderByDescending(row => row.Like - row.Dislike)
                  .ThenByDescending(row => row.Create_Date);
              break;
            default:
              orderedRows = rows.OrderByDescending(row => row.Create_Date);
              break;
          }
          return orderedRows.Skip(start)
                  .Take(CommentQuery.COMMENT_BLOCK_SIZE)
                  .Select(RowToComment).ToArray();
        });

      return comments;
    }

    public CRUDOperations ForNode(GDID gNode)
    {
      return DataStore.PartitionedOperationsFor(SocialConsts.MDB_AREA_NODE, gNode);
    }

    protected CRUDOperations ForComment(GDID gVolume)
    {
      return DataStore.PartitionedOperationsFor(SocialConsts.MDB_AREA_COMMENT, gVolume);
    }

    public Comment RowToComment(CommentRow row)
    {
      if (row == null)
        return default(Comment);
      var commentID = new CommentID(row.G_CommentVolume, row.GDID);
      var parentID = row.G_Parent.HasValue ? new CommentID(row.G_CommentVolume, row.G_Parent.Value) : (CommentID?)null;
      var authorNode = m_GraphSystemService.GetNode(row.G_AuthorNode);
      var targetNode = m_GraphSystemService.GetNode(row.G_TargetNode);
      var editableTimespan = GraphHost.EditCommentSpan(targetNode, row.Dimension);
      var lifeTime = App.TimeSource.UTCNow - row.Create_Date;

      return new Comment(commentID,
                         parentID,
                         authorNode,
                         targetNode,
                         row.Create_Date,
                         row.Dimension,
                         GSPublicationState.ToPublicationState(row.PublicationState),
                         row.IsRoot ? (RatingValue)row.Rating : RatingValue.Undefined,
                         row.Message,
                         row.Data,
                         row.Like,
                         row.Dislike,
                         row.ComplaintCount,
                         row.ResponseCount,
                         row.In_Use,
                         editableTimespan > lifeTime);
    }
  }
}
