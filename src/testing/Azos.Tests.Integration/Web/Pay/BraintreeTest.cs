/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.ApplicationModel;
using Azos.Financial;
using Azos.Scripting;
using Azos.Web.Pay;
using Azos.Web.Pay.Braintree;

namespace Azos.Tests.Integration.Web.Pay
{
  [Runnable]
  public class BraintreeTest : IRunnableHook
  {
    public const string LACONF = @"
    nfx
    {
      braintree-server-url='https://api.sandbox.braintreegateway.com'

      starters
      {
        starter
        {
          name='PaySystem'
          type='Azos.Web.Pay.PaySystemStarter, Azos.Web'
          application-start-break-on-exception=true
        }
      }

      web-settings
      {

        payment-processing
        {
          pay-system-host
          {
            name='FakePaySystemHost'
            type='Azos.Tests.Integration.Web.Pay.FakePaySystemHost, Azos.Tests.Integration'
            pay-system-prefix='BT'
            braintree-paymethod-token=$(~BRAINTREE_SANDBOX_PAYMETHOD_TOKEN)
          }

          pay-system
          {
            name='Braintree'
            type='Azos.Web.Pay.Braintree.BraintreeSystem, Azos.Web'
            auto-start=true

            api-uri=$(/$braintree-server-url)

            default-session-connect-params
            {
              name='Braintree'
              type='Azos.Web.Pay.Braintree.BraintreeConnectionParameters, Azos.Web'

              merchant-id=$(~BRAINTREE_SANDBOX_MERCHANT_ID)
              public-key=$(~BRAINTREE_SANDBOX_PUBLIC_KEY)
              private-key=$(~BRAINTREE_SANDBOX_PRIVATE_KEY)
            }
          }
        }
      }
    }";

    private ServiceBaseApplication m_App;

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

    [Run]
    public void ValidNonce()
    {
      Transaction tran = null;
      using (var session = BraintreeSystem.StartSession())
      {
        ((BraintreeSystem)BraintreeSystem).GenerateClientToken(session);

        var fromAccount = new Account("user", FakePaySystemHost.BRAINTREE_WEB_TERM, FakePaySystemHost.BRAINTREE_NONCE);
        var toAccount = Account.EmptyInstance;
        session.StoreAccountData(new ActualAccountData(fromAccount)
          {
            Identity = fromAccount.Identity,
            IsNew = true,
            IsWebTerminal = true,
            AccountID = fromAccount.AccountID,
            FirstName = "Stan",
            LastName = "Ulam",
            Phone = "(333) 777-77-77",
            EMail = "s-ulam@myime.com",
            BillingAddress = new Address { Address1 = "587 KIVA ST", PostalCode = "87544", City = "LOS ALAMOS", Region = "NM", Country = "USA" }
          });
        session.StoreAccountData(new ActualAccountData(toAccount));
        tran = session.Charge(fromAccount, toAccount, new Amount("usd", 99M), capture: false);
      }

      Aver.IsTrue(tran.Status == TransactionStatus.Success);
      Aver.IsTrue(tran.Type == TransactionType.Charge);
      Aver.AreObjectsEqual(tran.Amount, new Amount("usd", 99M));
      Aver.AreObjectsEqual(tran.AmountCaptured, new Amount("usd", 0M));
      Aver.IsTrue(tran.CanCapture);
      Aver.IsFalse(tran.CanRefund);
      Aver.IsTrue(tran.CanVoid);

      tran.Capture();
      Aver.IsTrue(tran.Status == TransactionStatus.Success);
      Aver.AreObjectsEqual(tran.Amount, new Amount("usd", 99M));
      Aver.AreObjectsEqual(tran.AmountCaptured, new Amount("usd", 99M));
      Aver.AreObjectsEqual(tran.AmountRefunded, new Amount("usd", 0M));
      Aver.IsFalse(tran.CanCapture);
      Aver.IsTrue(tran.CanRefund);
      Aver.IsFalse(tran.CanVoid);

      tran.Refund();
      Aver.IsTrue(tran.Status == TransactionStatus.Success);
      Aver.AreObjectsEqual(tran.Amount, new Amount("usd", 99M));
      Aver.AreObjectsEqual(tran.AmountCaptured, new Amount("usd", 99M));
      Aver.AreObjectsEqual(tran.AmountRefunded, new Amount("usd", 99M));
      Aver.IsFalse(tran.CanCapture);
      Aver.IsFalse(tran.CanRefund);
      Aver.IsFalse(tran.CanVoid);
    }

    [Run]
    [Aver.Throws(typeof(PaymentException), Message = "Expired Card", MsgMatch = Aver.ThrowsAttribute.MatchType.Exact)]
    public void DeclinedNonce()
    {
      Transaction tran = null;
      using (var session = BraintreeSystem.StartSession())
      {
        ((BraintreeSystem)BraintreeSystem).GenerateClientToken(session);

        var fromAccount = new Account("user", FakePaySystemHost.BRAINTREE_WEB_TERM, FakePaySystemHost.BRAINTREE_PROCESSOR_DECLINED_VISA_NONCE);
        var toAccount = Account.EmptyInstance;
        session.StoreAccountData(new ActualAccountData(fromAccount)
          {
            Identity = fromAccount.Identity,
            IsNew = true,
            IsWebTerminal = true,
            AccountID = fromAccount.AccountID,
            FirstName = "Stan",
            LastName = "Ulam",
            Phone = "(333) 777-77-77",
            EMail = "s-ulam@myime.com",
            BillingAddress = new Address { Address1 = "587 KIVA ST", PostalCode = "87544", City = "LOS ALAMOS", Region = "NM", Country = "USA" }
          });
        session.StoreAccountData(new ActualAccountData(toAccount));
        tran = session.Charge(fromAccount, toAccount, new Amount("usd", 2004M), capture: false);
      }
    }

    // to run this test you must have valid paymethod token in the braintree vault
    [Run]
    public void PaymethodToken()
    {
      if ((PaySystem.PaySystemHost as FakePaySystemHost).BraintreePayMethodToken.IsNullOrEmpty())
        Aver.Fail("System variable BRAINTREE_SANDBOX_PAYMETHOD_TOKEN is not set");

      Transaction tran = null;
      using (var session = BraintreeSystem.StartSession())
      {
        ((BraintreeSystem)BraintreeSystem).GenerateClientToken(session);

        var fromAccount = new Account("user", FakePaySystemHost.BRAINTREE_WEB_TERM, "PaymethodToken");
        var toAccount = Account.EmptyInstance;
        session.StoreAccountData(new ActualAccountData(fromAccount)
          {
            Identity = fromAccount.Identity,
            IsWebTerminal = false,
            AccountID = (PaySystem.PaySystemHost as FakePaySystemHost).BraintreePayMethodToken,
            FirstName = "Stan",
            LastName = "Ulam",
            Phone = "(333) 777-77-77",
            EMail = "s-ulam@myime.com"
          });
        session.StoreAccountData(new ActualAccountData(toAccount));
        tran = session.Charge(fromAccount, toAccount, new Amount("usd", 99M), capture: false);
      }

      Aver.IsTrue(tran.Status == TransactionStatus.Success);
      Aver.IsTrue(tran.Type == TransactionType.Charge);
      Aver.AreObjectsEqual(tran.Amount, new Amount("usd", 99M));
      Aver.AreObjectsEqual(tran.AmountCaptured, new Amount("usd", 0M));
      Aver.IsTrue(tran.CanCapture);
      Aver.IsFalse(tran.CanRefund);
      Aver.IsTrue(tran.CanVoid);

      tran.Capture();
      Aver.IsTrue(tran.Status == TransactionStatus.Success);
      Aver.AreObjectsEqual(tran.Amount, new Amount("usd", 99M));
      Aver.AreObjectsEqual(tran.AmountCaptured, new Amount("usd", 99M));
      Aver.AreObjectsEqual(tran.AmountRefunded, new Amount("usd", 0M));
      Aver.IsFalse(tran.CanCapture);
      Aver.IsTrue(tran.CanRefund);
      Aver.IsFalse(tran.CanVoid);

      tran.Refund();
      Aver.IsTrue(tran.Status == TransactionStatus.Success);
      Aver.AreObjectsEqual(tran.Amount, new Amount("usd", 99M));
      Aver.AreObjectsEqual(tran.AmountCaptured, new Amount("usd", 99M));
      Aver.AreObjectsEqual(tran.AmountRefunded, new Amount("usd", 99M));
      Aver.IsFalse(tran.CanCapture);
      Aver.IsFalse(tran.CanRefund);
      Aver.IsFalse(tran.CanVoid);

    }

    private IPaySystem m_PaySystem;
    public IPaySystem BraintreeSystem
    {
      get
      {
        if(m_PaySystem == null) { m_PaySystem = Azos.Web.Pay.PaySystem.Instances["Braintree"]; }
        return m_PaySystem;
      }
    }
  }
}
