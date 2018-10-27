/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Security;

namespace Azos.Web.Pay.Mock
{
  public class MockConnectionParameters: ConnectionParameters
  {
    #region Consts

      public const string MOCK_REALM = "MOCK_PAY_REALM";

      public const string CONFIG_ACCOUNTS_SECTION = "accounts";
      public const string CONFIG_ACCOUNT_ACTUAL_DATA_NODE = "account-actual-data";

      public const string CONFIG_EMAIL_ATTR = "email";

      public const string CONFIG_CARD_NUMBER_ATTR = "number";
      public const string CONFIG_CARD_EXPYEAR_ATTR = "exp-year";
      public const string CONFIG_CARD_EXPMONTH_ATTR = "exp-month";
      public const string CONFIG_CARD_CVC_ATTR = "cvc";

      public const string CONFIG_ACCOUNT_NUMBER_ATTR = "account-number";
      public const string CONFIG_ROUTING_NUMBER_ATTR = "routing-number";
      public const string CONFIG_ACCOUNT_TYPE_ATTR = "account-type";

    #endregion

    public MockConnectionParameters(): base() {}
    public MockConnectionParameters(IConfigSectionNode node): base(node) {}
    public MockConnectionParameters(string connectString, string format = Configuration.CONFIG_LACONIC_FORMAT)
      : base(connectString, format) {}

    //private MockActualAccountData[] m_AccountActualDatas;

    //public IEnumerable<MockActualAccountData> AccountActualDatas { get { return m_AccountActualDatas; } }

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      var email = node.AttrByName(CONFIG_EMAIL_ATTR).Value;
      var cred = new MockCredentials(email);
      var at = new AuthenticationToken(MOCK_REALM, email);

      User = new User(cred, at, email, Rights.None);

      //var nAccounts = node[CONFIG_ACCOUNTS_SECTION];
      //configureAccounts(nAccounts);
    }

    //private void configureAccounts(IConfigSectionNode nAccountActualDatas)
    //{
    //  var accountActualDatas = new List<MockActualAccountData>();

    //  foreach (var accountActualDataConf in nAccountActualDatas.Children.Where(c => c.IsSameName(CONFIG_ACCOUNT_ACTUAL_DATA_NODE)))
    //  {
    //    var actualAccountData = MockActualAccountData.MakeAndConfigure(accountActualDataConf);

    //    accountActualDatas.Add(actualAccountData);
    //  }

    //  m_AccountActualDatas = accountActualDatas.ToArray();
    //}

  } //MockConnectionParameters

}
