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

namespace Azos.Data.Heap
{
  /// <summary>
  /// Represents a globally-distributed replicated heap of eventually consistent instances of CvRDTs
  /// (Convergent Replicated Data Types)
  /// </summary>
  public interface IHeap : IApplicationComponent
  {
    /// <summary>
    /// Registry of areas of the heap. Each area is backed by a storage engine
    /// and has its own partitioning(sharding) rules
    /// </summary>
    IRegistry<IArea> Areas { get; }
  }

  /// <summary>
  /// Denotes IHeap implementation logic module
  /// </summary>
  public interface IHeapLogic : IHeap , IModule
  {
  }

}





