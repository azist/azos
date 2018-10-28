/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using Azos.Data.Access.Subscriptions;

namespace Azos.Data.Access.Erlang
{
  /// <summary>
  /// Implements mailboxes that receive data from Erlang CRUD data stores
  /// </summary>
  public class ErlCRUDMailbox : Mailbox
  {
    internal ErlCRUDMailbox(ErlDataStore store, string name) : base(store, name)
    {

    }
  }
}
