/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Collections;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Defines a logical division/area/namespace of the data heap.
  /// Each area has its own set of nodes which service it.
  /// All nodes must be EVENTUALLY deployed same assemblies.
  /// The replication is paused until all nodes run assemblies with the same BUILD-INFO,
  /// otherwise the data may not eventually converge
  /// </summary>
  public interface IArea : INamed
  {
    /// <summary>
    /// Directing heap which owns the area
    /// </summary>
    IHeap Heap { get; }

    /// <summary>
    /// Returns an enumerable of all types HeapObjects-derived types supported by this heap area instance
    /// </summary>
    IEnumerable<Type> ObjectTypes{ get; }

    /// <summary>
    /// Returns an enumerable of all types HeapQuery-derived types supported by this heap area instance
    /// </summary>
    IEnumerable<Type> QueryTypes { get; }

    /// <summary>
    /// Returns an unordered set of <see cref="INode"/> instances
    /// </summary>
    IEnumerable<INode> Nodes{ get; }

    /// <summary>
    /// Selects a node for the caller as determined by its settings.
    /// The system uses this handler to dispatch node calls to the best matching nodes
    /// </summary>
    INodeSelector NodeSelector { get; }

    /// <summary>
    /// Client used to connect to service
    /// </summary>
    Client.IHttpService ServiceClient {  get; }

    /// <summary>
    /// Returns a heap space for the specified object type
    /// </summary>
    ISpace GetSpace(Type tObject);

    /// <summary>
    /// Returns a heap space for the specified object type
    /// </summary>
    ISpace<T> GetSpace<T>() where T : HeapObject;

    /// <summary>
    /// Executes a query in this area
    /// </summary>
    /// <param name="query">Query object to execute</param>
    /// <param name="idempotencyToken"></param>
    /// <param name="node"></param>
    /// <returns>Returns result of polymorphic type (e.g. an enumerable of data documents) </returns>
    Task<SaveResult<object>> ExecuteAsync(HeapQuery query, Guid idempotencyToken = default(Guid), INode node = null);
  }

}





