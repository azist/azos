/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

namespace Azos.Apps.Volatile
{

  /// <summary>
  /// Implements ObjectStore service that does nothing on checkin, always returning null for checkout/fetch
  /// </summary>
  public sealed class NOPObjectStore : ApplicationComponent, IObjectStoreImplementation
  {

    internal NOPObjectStore(IApplication app): base(app){}

    public override string ComponentLogTopic => CoreConsts.OBJSTORE_TOPIC;
    public bool InstrumentationEnabled { get { return false; } set { } }
    public IEnumerable<KeyValuePair<string, Type>> ExternalParameters { get { return null; } }
    public IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups) { return null; }
    public bool ExternalGetParameter(string name, out object value, params string[] groups)
    {
      value = null;
      return false;
    }
    public bool ExternalSetParameter(string name, object value, params string[] groups)
    {
      return false;
    }

    public void Configure(Conf.IConfigSectionNode node)
    {

    }

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