/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;
using Azos.Log;


namespace Azos.Web.Pay
{
  /// <summary>
  /// Represents a process-global entity that resolves account handle into actual account data
  /// and fetches existing transactions.
  /// This design provides an indirection level between pay systems (like Stripe, PayPal, Bank etc.) and
  /// particular application data store implementation as it decouples system-internal formats of transaction and
  /// account storage from provider-internal data (i.e. PayPal payment token string).
  /// The instance of implementor is a singleton accessible via PaySystem.PaySystemHost
  /// </summary>
  public interface IPaySystemHost: Collections.INamed
  {
    /// <summary>
    /// Returns a transaction with specified ID from storage or null
    /// </summary>
    Transaction FetchTransaction(object id);

    /// <summary>
    /// Returns actual data for supplied account object
    /// </summary>
    IActualAccountData FetchAccountData(Account account);

    /// <summary>
    /// Currency market
    /// </summary>
    ICurrencyMarket CurrencyMarket { get; }
  }

  /// <summary>
  /// Denotes an implementation of IPaySystemHost
  /// </summary>
  public interface IPaySystemHostImplementation : IPaySystemHost, IConfigurable {}

  /// <summary>
  /// Denotes a context of transaction execution.
  /// Can be used to provide additional information
  /// </summary>
  public interface IPaySessionContext { }



#warning This class needs to be removed, use Interface, why is thic coupling?
  public abstract class PaySystemHost : DaemonWithInstrumentation<IApplicationComponent>, IPaySystemHostImplementation
  {
    #region CONST
    private const string LOG_TOPIC = "PaySystemHost";
    private const MessageType DEFAULT_LOG_LEVEL = MessageType.Warning;
    #endregion

    #region .ctor
    protected PaySystemHost(IApplication app) : base(app) { }

    protected PaySystemHost(IApplicationComponent director) : base(director) { }
    #endregion

    #region Properties
    /// <summary>
    /// Implements IInstrumentable
    /// </summary>
    public override bool InstrumentationEnabled { get { return false; } set { } }

    [Config(Default = DEFAULT_LOG_LEVEL)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_PAY)]
    public MessageType LogLevel { get; set; }
    #endregion

    public Transaction FetchTransaction(object id)
    { return DoFetchTransaction(null, id); }

    public IActualAccountData FetchAccountData(Account account)
    { return DoFetchAccountData(null, account); }

    public abstract ICurrencyMarket CurrencyMarket { get; }

    protected internal virtual IPaySessionContext GetDefaultTransactionContext() { return null; }

    protected internal abstract object DoGenerateTransactionID(PaySession session, TransactionType type);

    protected internal abstract Transaction DoFetchTransaction(PaySession session, object id);

    protected internal abstract IActualAccountData DoFetchAccountData(PaySession session, Account account);

    protected internal abstract void DoStoreTransaction(PaySession session, Transaction tran);

    protected internal abstract void DoStoreAccountData(PaySession session, IActualAccountData accoundData);
  }

}
