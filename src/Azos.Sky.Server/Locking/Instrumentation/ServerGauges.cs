/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Instrumentation;
using Azos.Serialization.Bix;
using Azos.Serialization.BSON;

namespace Azos.Sky.Locking.Instrumentation
{
  /// <summary>
  /// Provides the count of processed server transactions
  /// </summary>
  [Serializable]
  [Bix("3C295A45-28AC-487A-A040-D78B5D95C43D")]
  public sealed class ServerLockTransactions : LockingServerGauge
  {
    internal ServerLockTransactions(string src, long value) : base(src, value) { }

    public override string Description { get { return "Provides the count of processed server transactions "; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TRANSACTION; } }

    protected override Datum MakeAggregateInstance() { return new ServerLockTransactions(Source, 0); }
  }

  /// <summary>
  /// Provides the count of requests that end the session
  /// </summary>
  [Serializable]
  [Bix("88B2ACEA-59EC-4243-B862-27F7AEE1797C")]
  public sealed class ServerEndLockSessionCalls : LockingServerGauge
  {
    internal ServerEndLockSessionCalls(long value) : base(Datum.UNSPECIFIED_SOURCE, value) { }

    public override string Description { get { return "Provides the count of requests that end the session"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_CALL; } }

    protected override Datum MakeAggregateInstance() { return new ServerEndLockSessionCalls(0); }
  }

  /// <summary>
  /// Provides the current level of trust to locking server
  /// </summary>
  [Serializable]
  [Bix("7A0336B6-75B8-4625-BC40-9451D9411CA2")]
  public sealed class ServerTrustLevel : LockingServerDoubleGauge
  {
    internal ServerTrustLevel(double value) : base(Datum.UNSPECIFIED_SOURCE, value) { }

    public override string Description { get { return "Provides the current level of trust to locking server"; } }

    public override string ValueUnitName { get { return "trust"; } }

    protected override Datum MakeAggregateInstance() { return new ServerTrustLevel(0); }
  }

  /// <summary>
  /// Provides the current level of server calls which is considered a norm
  /// </summary>
  [Serializable]
  [Bix("DE39EC10-C8EB-4E97-9329-5C28E7793E95")]
  public sealed class ServerCallsNorm : LockingServerDoubleGauge
  {
    internal ServerCallsNorm(double value) : base(Datum.UNSPECIFIED_SOURCE, value) { }

    public override string Description { get { return "Provides the current level of server calls which is considered a norm"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_CALL; } }

    protected override Datum MakeAggregateInstance() { return new ServerCallsNorm(0); }
  }

  /// <summary>
  /// Provides the number of records that have expired
  /// </summary>
  [Serializable]
  [Bix("A219D4B6-8CAC-4D9D-9D85-F7E00B4A4365")]
  public sealed class ServerExpiredRecords : LockingServerGauge
  {
    internal ServerExpiredRecords(long value) : base(Datum.UNSPECIFIED_SOURCE, value) { }

    public override string Description { get { return "Provides the number of records that have expired"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_RECORD; } }

    protected override Datum MakeAggregateInstance() { return new ServerExpiredRecords(0); }
  }

  /// <summary>
  /// Provides the number of sessions that have expired
  /// </summary>
  [Serializable]
  [Bix("B1F5501B-C778-4583-83EA-F4EDFBF8570E")]
  public sealed class ServerExpiredSessions : LockingServerGauge
  {
    internal ServerExpiredSessions(long value) : base(Datum.UNSPECIFIED_SOURCE, value) { }

    public override string Description { get { return "Provides the number of sessions that have expired"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_SESSION; } }

    protected override Datum MakeAggregateInstance() { return new ServerExpiredSessions(0); }
  }

  /// <summary>
  /// Provides the number of empty tables removed from the server namespaces
  /// </summary>
  [Serializable]
  [Bix("19678A92-7F23-4FDB-95F3-DE2306063FC0")]
  public sealed class ServerRemovedEmptyTables : LockingServerGauge
  {
    internal ServerRemovedEmptyTables(long value) : base(Datum.UNSPECIFIED_SOURCE, value) { }

    public override string Description { get { return " Provides the number of empty tables removed from the server namespaces"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TABLE; } }

    protected override Datum MakeAggregateInstance() { return new ServerRemovedEmptyTables(0); }
  }

  /// <summary>
  /// Provides the count of tables per namespace
  /// </summary>
  [Serializable]
  [Bix("3EB81D57-C9A2-4245-B6D0-70A1E795D3AE")]
  public sealed class ServerNamespaceTables : LockingServerGauge
  {
    internal ServerNamespaceTables(string nsname, long value) : base(nsname, value) { }

    public override string Description { get { return "Provides the count of tables per namespace"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_TABLE; } }

    protected override Datum MakeAggregateInstance() { return new ServerNamespaceTables(this.Source, 0); }
  }

  /// <summary>
  /// Provides the count of committed records  per namespace table
  /// </summary>
  [Serializable]
  [Bix("56C447D2-D713-4D05-8F02-C448FB366174")]
  public sealed class ServerNamespaceTableRecordCount : LockingServerGauge
  {
    internal ServerNamespaceTableRecordCount(string nsTname, long value) : base(nsTname, value) { }

    public override string Description { get { return "Provides the count of committed records  per namespace table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_RECORD; } }

    protected override Datum MakeAggregateInstance() { return new ServerNamespaceTableRecordCount(this.Source, 0); }
  }
}
