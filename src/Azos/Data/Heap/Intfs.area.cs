/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Collections;
using Azos.Conf;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Provides information about objects participating in heap area functionality.
  /// Type schema is a set of types used for representing heap objects and heap queries.
  /// Resolves names to types.
  /// Schema detects version of assemblies where types are declared. The version is used to
  /// coordinate between nodes which run the same versions of data objects thus enabling their CRDT convergence.
  /// The nodes which run different versions are not replicated-from until the same type schema version
  /// gets installed on all nodes in the cluster ensuring eventual consistency backed by the same code.
  /// ATTENTION: All nodes of data heap must be installed THE SAME assembly binaries built on the same machine, otherwise
  /// the schema versions will be different and replication will not happen
  /// </summary>
  public interface ITypeSchema
  {
    /// <summary>
    /// An area which this schema is for
    /// </summary>
    IArea Area { get; }

    /// <summary>
    /// Provides the version digest - a hash value derived from all assemblies participating in this type schema.
    /// The system uses this value to coordinate versions of ITypeSchema instances deployed on every node in the cluster.
    /// ATTENTION: All nodes must be deployed the same set of assembly binaries built on the same machine (so their BuildInfo are identical)
    /// otherwise the heap data will not auto replicate
    /// </summary>
    ulong Version { get; }

    /// <summary>
    /// A list of (assemblyFQN:BuildInfo) tuples. The Version is derived from this enumerable
    /// </summary>
    IEnumerable<KeyValuePair<string, BuildInformation>> Assemblies { get; }

    /// <summary>
    /// All declared HeapObject-derived types decorated with [HeapSpace]
    /// </summary>
    IEnumerable<Type> ObjectTypes { get; }

    /// <summary>
    /// All declared HeapQuery-derived types decorated with [HeapProc]
    /// </summary>
    IEnumerable<Type> QueryTypes { get; }

    /// <summary>
    /// Maps space within area to HeapObject-derived type. Throws if not found
    /// </summary>
    Type MapObjectSpace(string space);

    /// <summary>
    /// Maps proc name within area to HeapQuery-derived types.
    /// Multiple query objects may be mapped to the same proc. Throws if not found
    /// </summary>
    IEnumerable<Type> MapQueryProc(string proc);
  }

  /// <summary>
  /// Defines a logical division/area/namespace of the data heap.
  /// Each area has its own set of nodes which service it.
  /// All nodes must be EVENTUALLY deployed the same assemblies.
  /// The replication is paused until all nodes run assemblies with the same BUILD-INFO,
  /// otherwise the data may not eventually converge
  /// </summary>
  public interface IArea : INamed, IApplicationComponent
  {
    /// <summary>
    /// Directing heap which owns the area
    /// </summary>
    IHeap Heap { get; }

    /// <summary>
    /// Returns type definition for this area
    /// </summary>
    ITypeSchema Schema { get; }

    /// <summary>
    /// Returns an unordered set of <see cref="INode"/> instances
    /// </summary>
    IEnumerable<INode> Nodes { get; }

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





