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
  /// Provides the count of processed server transactions
  /// </summary>
  [Serializable]
  [Arow("3C295A45-28AC-487A-A040-D78B5D95C43D")]
  [BSONSerializable("615ECDF5-0593-4F56-AD6D-5C4164D68A1B")]
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
  [Arow("88B2ACEA-59EC-4243-B862-27F7AEE1797C")]
  [BSONSerializable("51A3BBC6-7B33-4F01-9FA8-AEE8CA6EB665")]
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
  [Arow("7A0336B6-75B8-4625-BC40-9451D9411CA2")]
  [BSONSerializable("501689C3-B2AD-43C7-97D4-D6BDE7853EE4")]
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
  [Arow("DE39EC10-C8EB-4E97-9329-5C28E7793E95")]
  [BSONSerializable("C93A4229-C261-466A-B7A2-D77845BA7AC3")]
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
  [Arow("A219D4B6-8CAC-4D9D-9D85-F7E00B4A4365")]
  [BSONSerializable("51C49154-C017-480D-9CA7-1E9E20771F23")]
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
  [Arow("B1F5501B-C778-4583-83EA-F4EDFBF8570E")]
  [BSONSerializable("4D7295AC-FE3C-4382-B698-D4F00522FC52")]
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
  [Arow("19678A92-7F23-4FDB-95F3-DE2306063FC0")]
  [BSONSerializable("811ED7C5-E27F-474D-BC4D-7DE4A0303668")]
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
  [Arow("3EB81D57-C9A2-4245-B6D0-70A1E795D3AE")]
  [BSONSerializable("988FC14B-4631-472C-8252-E30AD78C8369")]
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
  [Arow("56C447D2-D713-4D05-8F02-C448FB366174")]
  [BSONSerializable("2F85C218-D985-41F9-838F-FBED90BAC563")]
  public sealed class ServerNamespaceTableRecordCount : LockingServerGauge
  {
    internal ServerNamespaceTableRecordCount(string nsTname, long value) : base(nsTname, value) { }

    public override string Description { get { return "Provides the count of committed records  per namespace table"; } }

    public override string ValueUnitName { get { return CoreConsts.UNIT_NAME_RECORD; } }

    protected override Datum MakeAggregateInstance() { return new ServerNamespaceTableRecordCount(this.Source, 0); }
  }
}
