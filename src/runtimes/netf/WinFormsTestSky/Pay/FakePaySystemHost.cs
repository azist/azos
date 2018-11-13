using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

using Azos;
using Azos.Conf;
using Azos.Web.Pay;
using Azos.Data;
using Azos.Web.Pay.Mock;
using Azos.Serialization.Slim;
using Azos.Financial;

namespace WinFormsTestSky.Pay
{
  internal class FakePaySystemHost : PaySystemHost
  {
    #region Consts
    public readonly static Account CARD_ACCOUNT_STRIPE_CORRECT = new Account("user", 111, 1000001);
    public readonly static Account CARD_DECLINED = new Account("user", 111, 1000100);
    public readonly static Account CARD_LUHN_ERR = new Account("user", 111, 1000101);
    public readonly static Account CARD_EXP_YEAR_ERR = new Account("user", 111, 1000102);
    public readonly static Account CARD_EXP_MONTH_ERR = new Account("user", 111, 1000103);
    public readonly static Account CARD_CVC_ERR = new Account("user", 111, 1000104);

    public readonly static Account CARD_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS = new Account("user", 111, 1000002);
    public readonly static Account CARD_DEBIT_ACCOUNT_STRIPE_CORRECT = new Account("user", 111, 1000003);
    public readonly static Account CARD_DEBIT_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS = new Account("user", 111, 1000004);
    public readonly static Account BANK_ACCOUNT_STRIPE_CORRECT = new Account("user", 111, 2000001);
    #endregion

    #region Static
      private static Lazy<FakePaySystemHost> s_Instance = new Lazy<FakePaySystemHost>(() => new FakePaySystemHost());

      public static FakePaySystemHost Instance { get { return s_Instance.Value; } }
    #endregion

    #region Accounts hardcoded

      public List<IActualAccountData> MockActualAccountDatas = new List<IActualAccountData> {
        new MockActualAccountData() {
          Account = CARD_ACCOUNT_STRIPE_CORRECT,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4242424242424242",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "123"
          }
        },

        new MockActualAccountData() {
          Account = CARD_DECLINED,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4000000000000002",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "123"
          }
        },

        new MockActualAccountData() {
          Account = CARD_LUHN_ERR,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",
            AccountNumber = "4242424242424241",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "123"
          }
        },

        new MockActualAccountData() {
          Account = CARD_EXP_YEAR_ERR,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4242424242424242",
            CardExpirationYear = 1970,
            CardExpirationMonth = 11,
            CardVC = "123"
          }
        },

        new MockActualAccountData() {
          Account = CARD_EXP_MONTH_ERR,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4242424242424242",
            CardExpirationYear = 2016,
            CardExpirationMonth = 13,
            CardVC = "123"
          }
        },

        new MockActualAccountData() {
          Account = CARD_CVC_ERR,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4242424242424242",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "99"
          }
        },

        new MockActualAccountData() {
          Account = CARD_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4242424242424242",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "123",

            BillingAddress1= "5844 South Oak Street",
            BillingAddress2 = "1234 Lemon Street",
            BillingCountry = "US",
            BillingCity = "Chicago",
            BillingPostalCode = "60667",
            BillingRegion = "IL",
            BillingEmail = "vpupkin@mail.com",
            BillingPhone = "(309) 123-4567"
          }
        },

        new MockActualAccountData() {
          Account = BANK_ACCOUNT_STRIPE_CORRECT,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "000123456789",
            RoutingNumber = "110000000",

            BillingCountry = "US"
          }
        },

        new MockActualAccountData() {
          Account = CARD_DEBIT_ACCOUNT_STRIPE_CORRECT,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4000056655665556",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "123"
          }
        },

        new MockActualAccountData() {
          Account = CARD_DEBIT_ACCOUNT_STRIPE_CORRECT_WITH_ADDRESS,
          AccountData = new AccountData()
          {
            FirstName = "Vasya",
            LastName = "Pupkin",
            MiddleName = "S",

            AccountNumber = "4000056655665556",
            CardExpirationYear = 2016,
            CardExpirationMonth = 11,
            CardVC = "123",

            BillingAddress1= "5844 South Oak Street",
            BillingAddress2 = "1234 Lemon Street",
            BillingCountry = "US",
            BillingCity = "Chicago",
            BillingPostalCode = "60667",
            BillingRegion = "IL",
            BillingEmail = "vpupkin@mail.com",
            BillingPhone = "(309) 123-4567"
          }
        },
      };

    #endregion

    #region .ctor
    public FakePaySystemHost() : base(typeof(FakePaySystemHost).Name, null) { }
    public FakePaySystemHost(string name, IConfigSectionNode node) : base(name, node) { }
    public FakePaySystemHost(string name, IConfigSectionNode node, object director) : base(name, node, director) { }
    #endregion

    #region Fields
    private static SlimSerializer m_Serializer = new SlimSerializer();

      private static List<Transaction> m_TransactionList = new List<Transaction>();

      private static ConcurrentDictionary<object, byte[]> m_Transactions = new ConcurrentDictionary<object,byte[]>();

      private ICurrencyMarket m_CurrencyMarket = new Azos.Web.Pay.ConfigBasedCurrencyMarket();
    #endregion

    #region IPaySystemHostImplementation
      public override ICurrencyMarket CurrencyMarket { get { return m_CurrencyMarket; } }

      protected override object DoGenerateTransactionID(PaySession session, TransactionType type)
      { return generateUniqueID(); }

      /// <summary>
      /// In this implementation returns transaction from memory list by id
      /// </summary>
      protected override Transaction DoFetchTransaction(PaySession session, object id)
      {
        byte[] buf;
        if (!m_Transactions.TryGetValue(id, out buf)) throw new AzosException("Couldn't find transaction by id '{0}'".Args(id));

        var ta = deserializeTransaction(buf);
        return ta;
      }

      protected override IActualAccountData DoFetchAccountData(PaySession session, Account account)
      {
        return MockActualAccountDatas.FirstOrDefault(m => m.Account == account);
      }

      protected override void DoStoreTransaction(PaySession session, Transaction tran)
      {
        var buf = serializeTransaction(tran);

        m_Transactions.AddOrUpdate(tran.ID, buf, (buf0, buf1) => { return buf; });
      }

      protected override void DoStoreAccountData(PaySession session, IActualAccountData accoundData)
      {
        MockActualAccountDatas.RemoveAll(ad => ad.Account == accoundData.Account);
        MockActualAccountDatas.Add(accoundData);
      }
    #endregion

    #region Private
      private string generateUniqueID()
      {
        ulong id = (((ulong)App.Random.NextRandomInteger) << 32) + ((ulong)App.Random.NextRandomInteger);
        var eLink = new ELink(id, new byte[] { });
        return eLink.Link;
      }

      private byte[] serializeTransaction(Transaction ta)
      {
        lock(m_Serializer)
        {
          using (var ms = new MemoryStream())
          {
            m_Serializer.Serialize(ms, ta);
            var buf = new byte[ms.Position];
            Array.Copy(ms.GetBuffer(), 0, buf, 0, ms.Position);
            return buf;
          }
        }
      }

      private Transaction deserializeTransaction(byte[] buf)
      {
        lock (m_Serializer)
        {
          using (var ms = new MemoryStream(buf))
          {
            var ta = (Transaction)m_Serializer.Deserialize(ms);
            return ta;
          }
        }
      }
    #endregion
  }
}
