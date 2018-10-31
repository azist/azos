using System;

using Azos.Instrumentation;
using Azos.Serialization.BSON;

namespace Azos.Sky.Locking.Instrumentation
{


  /// <summary>
  /// Provides the count of LockSession instances in the LockManager
  /// </summary>
  [Serializable]
  [BSONSerializable("C189E810-9971-4225-94C3-306FEFB40600")]
  public sealed class LockSessions : LockingClientGauge
  {
    internal LockSessions(long value) : base(Datum.UNSPECIFIED_SOURCE, value) { }

    public override string Description { get { return "Provides the count of LockSession instances in the LockManager"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_SESSION; } }

    protected override Datum MakeAggregateInstance() { return new LockSessions(0); }
  }


  /// <summary>
  /// Provides the number of times that locking transactions have been requested to be executed
  /// </summary>
  [Serializable]
  [BSONSerializable("06A5C7D0-7A28-4E8C-B642-B089B33717C1")]
  public sealed class LockTransactionRequests : LockingClientGauge
  {
    internal LockTransactionRequests(string src, long value) : base(src, value) { }

    public override string Description
    {
      get { return "Provides the number of times that locking transactions have been requested to be executed"; }
    }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TRANSACTION; } }

    protected override Datum MakeAggregateInstance() { return new LockTransactionRequests(this.Source, 0); }
  }
}
