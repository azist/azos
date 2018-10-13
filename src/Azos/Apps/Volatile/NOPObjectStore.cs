
using System;

namespace Azos.Apps.Volatile
{

  /// <summary>
  /// Implements ObjectStore service that does nothing on checkin, always returning null for chekout/fetch
  /// </summary>
  public sealed class NOPObjectStore : ApplicationComponent, IObjectStore
  {

    public NOPObjectStore(): base(){}


    public object CheckOut(Guid key) => null;
    public bool UndoCheckout(Guid key) => false;
    public void CheckIn(Guid key, object value, int msTimeout = 0)
    {

    }

    /// <summary>
    /// Puts an object reference "value" into store identified by the "oldKey" under the "newKey".
    /// If oldKey was not checked in, then checks-in under new key as normally would
    /// </summary>
    public void CheckInUnderNewKey(Guid oldKey, Guid newKey, object value, int msTimeout = 0)
    {

    }

    /// <summary>
    /// Puts an object reference "value" into store identified by the "key"
    /// </summary>
    public bool CheckIn(Guid key, int msTimeout = 0) => true;
    public bool Delete(Guid key) => false;

    private static readonly NOPObjectStore Instance = new NOPObjectStore();



    public object Fetch(Guid key, bool touch = false) => null;

    public int ObjectLifeSpanMS => 0;

    public Time.TimeLocation TimeLocation => App.TimeLocation;

    public DateTime LocalizedTime => App.LocalizedTime;

    public DateTime UniversalTimeToLocalizedTime(DateTime utc)
    {
        return App.UniversalTimeToLocalizedTime(utc);
    }

    public DateTime LocalizedTimeToUniversalTime(DateTime local)
    {
        return App.LocalizedTimeToUniversalTime(local);
    }
  }

}