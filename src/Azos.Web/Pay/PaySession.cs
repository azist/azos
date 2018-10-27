/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Collections;
using Azos.Security;
using Azos.Financial;
using System.Collections.Generic;

namespace Azos.Web.Pay
{
  /// <summary>
  /// Represents session of PaySystem.
  /// All PaySystem operation requires session as mandatory parameter
  /// </summary>
  public abstract class PaySession : DisposableObject, INamed
  {
    #region .ctor
    protected PaySession(PaySystem paySystem, ConnectionParameters cParams, IPaySessionContext context = null)
    {
      if (paySystem == null || cParams == null)
        throw new PaymentException(StringConsts.ARGUMENT_ERROR + this.GetType().Name + ".ctor((paySystem|cParams)=null)");

      PaySystem = paySystem;

      Context = context ?? PaySystemHost.GetDefaultTransactionContext();

      ConnectionParameters = cParams;

      lock (PaySystem.Sessions)
        PaySystem.Sessions.Add(this);
    }

    protected override void Destructor()
    {
      try
      {
        if (m_AffectedAccounts != null)
          foreach (var account in m_AffectedAccounts.Values)
            PaySystemHost.DoStoreAccountData(this, account);

        if (m_AffectedTransactions != null)
          foreach (var tran in m_AffectedTransactions.Values)
            PaySystemHost.DoStoreTransaction(this, tran);
      }
      finally
      {
        if (PaySystem != null)
          lock (PaySystem.Sessions)
            PaySystem.Sessions.Remove(this);
      }

      base.Destructor();
    }
    #endregion

    #region Fields
    public readonly IPaySessionContext Context;
    public readonly PaySystem PaySystem;
    protected readonly ConnectionParameters ConnectionParameters;

    private Dictionary<object, Transaction> m_AffectedTransactions;
    private Dictionary<Account, IActualAccountData> m_AffectedAccounts;
    #endregion

    #region Properties
    protected PaySystemHost PaySystemHost { get { return PaySystem.PaySystemHost as PaySystemHost; } }
    public string Name { get { return ConnectionParameters.Name; } }
    public User User { get { return ConnectionParameters.User; } internal set { ConnectionParameters.User = value; } }

    public bool IsValid { get { return ConnectionParameters.User != null && ConnectionParameters.User != User.Fake; } }
    #endregion

    #region Public
    /// <summary>
    /// Has the same semantics as corresponding PaySystem method executed in context of this session
    /// </summary>
    public PaymentException VerifyPotentialTransaction(TransactionType type, Account from, Account to, Amount amount)
    {
      return PaySystem.DoVerifyPotentialTransaction(this, type, from, to, amount);
    }

    /// <summary>
    /// Has the same semantics as corresponding PaySystem method executed in context of this session
    /// </summary>
    public Transaction Charge(Account from, Account to, Amount amount, bool capture = true, string description = null, object extraData = null)
    {
      var tran = PaySystem.DoCharge(this, from, to, amount, capture, description, extraData);
      StoreTransaction(tran);
      return tran;
    }

    /// <summary>
    /// Has the same semantics as corresponding PaySystem method executed in context of this session
    /// </summary>
    public Transaction Transfer(Account from, Account to, Amount amount, string description = null, object extraData = null)
    {
      var tran = PaySystem.DoTransfer(this, from, to, amount, description, extraData);
      StoreTransaction(tran);
      return tran;
    }

    /// <summary>
    /// Has the same semantics as corresponding PaySystem method executed in context of this session
    /// </summary>
    public bool Refresh(Transaction tran)
    {
      var changed = PaySystem.DoRefresh(this, tran);
      if (changed) StoreTransaction(tran);
      return changed;
    }

    /// <summary>
    /// Has the same semantics as corresponding PaySystem method executed in context of this session
    /// </summary>
    public bool Void(Transaction tran, string description = null, object extraData = null)
    {
      var changed = PaySystem.DoVoid(this, tran, description, extraData);
      if (changed) StoreTransaction(tran);
      return changed;
    }

    /// <summary>
    /// Has the same semantics as corresponding PaySystem method executed in context of this session
    /// </summary>
    public bool Capture(Transaction tran, decimal? amount = null, string description = null, object extraData = null)
    {
      var changed = PaySystem.DoCapture(this, tran, amount, description, extraData);
      if (changed) StoreTransaction(tran);
      return changed;
    }

    /// <summary>
    /// Has the same semantics as corresponding PaySystem method executed in context of this session
    /// </summary>
    public bool Refund(Transaction tran, decimal? amount = null, string description = null, object extraData = null)
    {
      var changed = PaySystem.DoRefund(this, tran, amount, description, extraData);
      if (changed) StoreTransaction(tran);
      return changed;
    }

    /// <summary>
    /// Generates new transaction ID for desired pay session and transaction type (Charge, Transfer).
    /// Context supplies host specific information about this transation i.e. user id
    /// </summary>
    public object GenerateTransactionID(TransactionType type)
    {
      return PaySystemHost.DoGenerateTransactionID(this, type);
    }

    public void StoreTransaction(Transaction tran)
    {
      if (m_AffectedTransactions == null)
        m_AffectedTransactions = new Dictionary<object, Transaction>();

      m_AffectedTransactions[tran.ID] = tran;
    }

    public void StoreAccountData(IActualAccountData accoundData)
    {
      if (m_AffectedAccounts == null)
        m_AffectedAccounts = new Dictionary<Account, IActualAccountData>();

      m_AffectedAccounts[accoundData.Account] = accoundData;
    }

    public Transaction FetchTransaction(object id)
    {
      Transaction result = null;
      if (m_AffectedTransactions != null && m_AffectedTransactions.ContainsKey(id))
        result = m_AffectedTransactions[id];
      if (result == null)
        result = PaySystemHost.DoFetchTransaction(this, id);
      return result;
    }

    public IActualAccountData FetchAccountData(Account account)
    {
      IActualAccountData result = null;
      if (m_AffectedAccounts != null && m_AffectedAccounts.ContainsKey(account))
        result = m_AffectedAccounts[account];
      if (result == null)
        result = PaySystemHost.DoFetchAccountData(this, account);
      return result;
    }
    #endregion
  }
}
