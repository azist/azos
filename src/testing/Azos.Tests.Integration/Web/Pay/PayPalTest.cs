/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Threading;

using Azos.ApplicationModel;
using Azos.Financial;
using Azos.Scripting;
using Azos.Web.Pay;
using Azos.Web.Pay.PayPal;
using static Azos.Aver.ThrowsAttribute;

namespace Azos.Tests.Integration.Web.Pay
{
  [Runnable]
  public class PayPalTest : IRunnableHook
  {
    #region CONFIGURATION
    public const string LACONF = @"
    nfx
    {
      paypal-server-url='https://api.sandbox.paypal.com'

      starters
      {
        starter
        {
          name='Pay Systems'
          type='Azos.Web.Pay.PaySystemStarter, Azos.Web'
          application-start-break-on-exception=true
        }
      }

      web-settings
      {
        service-point-manager
        {
          security-protocol=4032 // Tls|Tls11|Tls12 binary flag

          service-point { uri=$(/$paypal-server-url) expect-100-continue=true }

          policy {}
        }

        payment-processing
        {
          pay-system-host
          {
            name='PayPalHost'
            type='Azos.Tests.Integration.Web.Pay.FakePaySystemHost, Azos.Tests.Integration'

            paypal-valid-account=$(~PAYPAL_SANDBOX_VALID_ACCOUNT)
          }

          pay-system
          {
            name='PayPalSys'
            type='Azos.Web.Pay.PayPal.PayPalSystem, Azos.Web'
            auto-start=true

            api-uri=$(/$paypal-server-url)
            sync-mode=false

            payout-email-subject='Payout from Azos PayPalTest'
            payout-note='Thanks for using Azos PayPalTest'

            default-session-connect-params
            {
              name='PayPalPayouts'
              type='Azos.Web.Pay.PayPal.PayPalConnectionParameters, Azos.Web'

              client-id=$(~PAYPAL_SANDBOX_CLIENT_ID)
              client-secret=$(~PAYPAL_SANDBOX_CLIENT_SECRET)
            }
          }
        }
      }
    }";
    #endregion

    [Run]
    public void PayoutTest()
    {
      var ps = PayPalSys;

      var amount = new Amount("USD", 1.0m);
      var from = new Account("SYSTEM", 111, 222);
      var to = new Account("USER", 111, 222);

      object id = null;
      using (var session = ps.StartSession())
      {
        session.StoreAccountData(new ActualAccountData(from)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = from.AccountID
        });

        session.StoreAccountData(new ActualAccountData(to)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = PayPalHost.PaypalValidAccount
        });

        var tran = session.Transfer(from, to, amount);
        Aver.IsNotNull(tran);
        Aver.IsNotNull(tran.ID);
        id = tran.ID;
        Aver.IsTrue(TransactionType.Transfer == tran.Type);
        Aver.AreEqual(amount, tran.Amount);
        Aver.AreEqual(from, tran.From);
        Aver.AreEqual(to, tran.To);
        Aver.IsTrue(TransactionStatus.Promised == tran.Status);

        Aver.IsNotNull(((PayPalSession)session).AuthorizationToken); // token occured on first call
        var token = session.User.AuthToken.Data as PayPalOAuthToken;
        Aver.IsNotNull(token);
        Aver.IsTrue(token.ObtainTime > App.TimeSource.Now.AddMinutes(-1));
        Aver.AreEqual(3600, token.ExpirationMarginSec);
        Aver.IsTrue(token.ApplicationID.IsNotNullOrEmpty());
        Aver.IsTrue(token.ExpiresInSec > 0);
        Aver.IsTrue(token.AccessToken.IsNotNullOrEmpty());
        Aver.IsTrue(token.Scope.IsNotNullOrEmpty());
        Aver.IsTrue(token.Nonce.IsNotNullOrEmpty());
      }

      var transaction = PayPalHost.FetchTransaction(id);
      Aver.IsNotNull(transaction);

      // actually, the transfer is made only when Refresh() method is calling
      refreshTransaction(transaction);
      Aver.IsNotNull(transaction);
      Aver.IsTrue(TransactionType.Transfer == transaction.Type);
      Aver.AreEqual(amount, transaction.Amount);
      Aver.AreEqual(from, transaction.From);
      Aver.AreEqual(to, transaction.To);
      Aver.IsTrue(TransactionStatus.Pending == transaction.Status);

      refreshTransaction(transaction);
      Aver.IsNotNull(transaction);
      Aver.IsTrue(TransactionType.Transfer == transaction.Type);
      Aver.AreEqual(amount, transaction.Amount);
      Aver.AreEqual(from, transaction.From);
      Aver.AreEqual(to, transaction.To);
      Aver.IsTrue(TransactionStatus.Success == transaction.Status);
    }

    [Run]
    [Aver.Throws(typeof(PaymentException), Message = "ITEM_INCORRECT_STATUS", MsgMatch = MatchType.Contains)]
    public void VoidSuccessedPayoutTest()
    {
      var ps = PayPalSys;

      var amount = new Amount("USD", 1.1m);
      var from = new Account("SYSTEM", 111, 222);
      var to = new Account("USER", 111, 222);

      object id = null;
      using (var session = ps.StartSession())
      {
        session.StoreAccountData(new ActualAccountData(from)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = from.AccountID
        });
        session.StoreAccountData(new ActualAccountData(to)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = PayPalHost.PaypalValidAccount
        });
        var tran = session.Transfer(from, to, amount);
        id = tran.ID;
      }

      var transaction = PayPalHost.FetchTransaction(id);
      Aver.IsNotNull(transaction);

      refreshTransaction(transaction);
      Aver.IsTrue(TransactionStatus.Pending == transaction.Status);

      refreshTransaction(transaction);
      Aver.IsTrue(TransactionStatus.Success == transaction.Status);

      transaction.Void();
    }

    [Run]
    [Aver.Throws(typeof(PaymentException), Message = "USER_BUSINESS_ERROR", MsgMatch = MatchType.Contains)]
    public void DuplicatePayoutTest()
    {
      var ps = PayPalSys;

      var amount = new Amount("USD", 1.2m);
      var from = new Account("SYSTEM", 111, 222);
      var to = new Account("USER", 111, 222);

      object id = null;
      using (var session = ps.StartSession())
      {
        session.StoreAccountData(new ActualAccountData(from)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = from.AccountID
        });
        session.StoreAccountData(new ActualAccountData(to)
        {
          Identity = from.Identity,
          IdentityID = from.IdentityID,
          AccountID = PayPalHost.PaypalValidAccount
        });
        var tran = session.Transfer(from, to, amount);
        id = tran.ID;
      }

      var transaction = PayPalHost.FetchTransaction(id);
      refreshTransaction(transaction);

      //refreshTransaction(transaction);
      //Aver.IsTrue(TransactionStatus.Success == transaction.Status);

      PayPalHost.SetNextTransactionID(id);

      using (var session = ps.StartSession())
        session.Transfer(from, to, amount);

      transaction = PayPalHost.FetchTransaction(id);
      refreshTransaction(transaction);
    }

    private ServiceBaseApplication m_App;

    private IPaySystem m_PayPalSys;
    public IPaySystem PayPalSys
    {
      get
      {
        if (m_PayPalSys == null) { m_PayPalSys = PaySystem.Instances["PayPalSys"]; }
        return m_PayPalSys;
      }
    }

    internal FakePaySystemHost PayPalHost
    {
      get
      {
        return PaySystem.PaySystemHost as FakePaySystemHost;
      }
    }


    void IRunnableHook.Prologue(Runner runner, FID id)
    {
      var config = LACONF.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
      m_App = new ServiceBaseApplication(null, config);
    }

    bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error)
    {
      DisposableObject.DisposeAndNull(ref m_App);
      return false;
    }

    private void refreshTransaction(Transaction transaction)
    {
      for (var i = 0; i < 5; i++)
      {
        var sleep = ExternalRandomGenerator.Instance.NextScaledRandomInteger(5000, 8000);
        Thread.Sleep(sleep);
        if (transaction.Refresh()) return;
      }
    }
  }
}
