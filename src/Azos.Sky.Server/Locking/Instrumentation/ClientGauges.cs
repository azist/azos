/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Instrumentation;
using Azos.Serialization.Arow;
using Azos.Serialization.BSON;

namespace Azos.Sky.Locking.Instrumentation
{


  /// <summary>
  /// Provides the count of LockSession instances in the LockManager
  /// </summary>
  [Serializable]
  [Arow("4258663D-21C0-48CB-9CFD-4EB335D923CC")]
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
  [Arow("4EF3D2B7-C4EF-4F87-985E-B3D85DDB5EA0")]
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
