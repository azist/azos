/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Security;

namespace Azos.Web.Pay.Braintree
{
  public class BraintreeConnectionParameters : ConnectionParameters
  {
    #region .ctor
    public BraintreeConnectionParameters(): base() { }
    public BraintreeConnectionParameters(IConfigSectionNode node): base(node) { }
    public BraintreeConnectionParameters(string connectString, string format = Configuration.CONFIG_LACONIC_FORMAT)
      : base(connectString, format) { }
    #endregion

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      User = User.Fake;

      var merchantId = node.AttrByName("merchant-id").ValueAsString();
      if (merchantId.IsNullOrWhiteSpace()) return;

      var publicKey = node.AttrByName("public-key").ValueAsString();
      var privateKey = node.AttrByName("private-key").ValueAsString();
      if (publicKey.IsNullOrWhiteSpace() || privateKey.IsNullOrWhiteSpace()) return;

      User = new User(new BraintreeCredentials(merchantId, publicKey, privateKey), new AuthenticationToken(BraintreeSystem.BRAINTREE_REALM, null), merchantId, Rights.None);
    }
  }
}