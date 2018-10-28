/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data.Access.Subscriptions;
using Azos.Erlang;

namespace Azos.Data.Access.Erlang
{
  /// <summary>
    /// Provides query execution environment in Erlang context
    /// </summary>
    public struct ErlCRUDQueryExecutionContext : ICRUDQueryExecutionContext
    {
        public ErlCRUDQueryExecutionContext(ErlDataStore store, ErlMbox erlMBox = null, DataTimeStamp? ts = null)
        {
          this.DataStore = store;
          ErlMailBox = erlMBox;
          SubscriptionTimestamp = ts;
        }

        public readonly ErlDataStore  DataStore;
        public readonly ErlMbox ErlMailBox;
        public readonly DataTimeStamp? SubscriptionTimestamp;
    }
}
