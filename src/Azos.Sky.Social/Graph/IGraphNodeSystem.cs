
using Azos.Data;
using Azos.Glue;

using Azos.Sky.Contracts;

namespace Azos.Sky.Social.Graph
{
  /// <summary>
  /// Handles the base social graph functionality such as CRUD of graph nodes (users, forums, groups etc..)
  /// </summary>
  [Glued]
  public interface IGraphNodeSystem : ISkyService
  {
    /// <summary>
    /// Saves the GraphNode instances into the system.
    /// If a node with such ID already exists, updates it, otherwise creates a new node
    /// Return GDID Node
    /// </summary>
    GraphChangeStatus SaveNode(GraphNode node);

    /// <summary>
    /// Fetches the GraphNode by its unique GDID or unassigned node if not found
    /// </summary>
    GraphNode GetNode(GDID gNode);

    /// <summary>
    /// Deletes node by GDID
    /// </summary>
    GraphChangeStatus DeleteNode(GDID gNode);

    /// <summary>
    /// Un-deletes node by GDID
    /// </summary>
    GraphChangeStatus UndeleteNode(GDID gNode);

    /// <summary>
    /// Physically removes node by GDID from database
    /// </summary>
    GraphChangeStatus RemoveNode(GDID gNode);
  }

  /// <summary>
  /// Contract for client of IGraphSystem svc
  /// </summary>
  public interface IGraphNodeSystemClient : ISkyServiceClient, IGraphNodeSystem
  {
    //todo Add async versions
  }
}
