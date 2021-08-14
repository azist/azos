/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Collections;

namespace Azos.Data.Access
{
  /// <summary>
  /// Provides higher-order data store hosting services.
  /// This entity unifies data access to various named da
  /// </summary>
  public interface IDataStoreHub : IDataStore
  {
    /// <summary> Registry of data stores </summary>
    IRegistry<IDataStore> DataStores { get; }
  }


  public interface IDataStoreHubImplementation : IDataStoreHub, IDataStoreImplementation { }
}
