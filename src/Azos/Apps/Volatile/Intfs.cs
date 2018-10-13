
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.Time;
using Azos.Instrumentation;


namespace Azos.Apps.Volatile
{
  /// <summary>
  /// Outlines interface for object stores. Object stores are special kind of zero-latency storage of application state
  /// which needs to be persisted between application/process restart (hence the name "volatile process") with expiration.
  /// For example, an app may keep user session context (or any other call context)in this store to ensure the survival
  /// of these contexts between voilatile app lifecycle.
  /// Azos framework provides an abstraction of store "providers" which store objects in
  /// files, database or other media. The usage of Object stores unifies the access to such data.
  /// Object stores are by-design expected to be in-memory/fast, therefore they do not expose ASYNC apis.
  /// The writing of objects to persistent media (such as disk) is expected to be done asynchronously by other thread/s
  /// </summary>
  public interface IObjectStore : IApplicationComponent, ILocalizedTimeProvider
  {

    /// <summary>
    /// Retrieves an object reference from the store identified by the "key" or returns null if such object does not exist.
    /// Object is not going to be persisted as this method provides logical read-only access. If touch=true then object timestamp is updated
    /// </summary>
    object Fetch(Guid key, bool touch = false);


    /// <summary>
    /// Retrieves an object reference from the store identified by the "key" or returns null if such object does not exist.
    /// Object is not going to be persisted until it is checked back in the store.
    /// </summary>
    object CheckOut(Guid key);


    /// <summary>
    /// Reverts object state to Normal after the call to Checkout. This way the changes (if any) are not going to be persisted.
    /// Returns true if object was found and checkout canceled. Keep in mind: this method CAN NOT revert inner object state
    ///  to its original state if it was changed, it only unmarks object as changed.
    /// This method is reentrant just like the Checkout is
    /// </summary>
    bool UndoCheckout(Guid key);

    /// <summary>
    /// Puts an object into store identified by the "key"
    /// </summary>
    bool CheckIn(Guid key, int msTimeout = 0);


    /// <summary>
    /// Puts an object reference "value" into store identified by the "key"
    /// </summary>
    void CheckIn(Guid key, object value, int msTimeout = 0);

    /// <summary>
    /// Puts an object reference "value" into store identified by the "oldKey" under the "newKey".
    /// If oldKey was not checked in, then checks-in under new key as normally would
    /// </summary>
    void CheckInUnderNewKey(Guid oldKey, Guid newKey, object value, int msTimeout = 0);


    /// <summary>
    /// Deletes object identified by key. Returns true when object was found and marked for deletion
    /// </summary>
    bool Delete(Guid key);

    /// <summary>
    /// Specifies how long objects live without being touched before becoming evicted from the store
    /// </summary>
    int ObjectLifeSpanMS{ get; }
  }


  public interface IObjectStoreImplementation : IObjectStore, IDisposable, IConfigurable, IInstrumentable
  {

  }


}
