

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
