/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using Azos.Conf;
using Azos.Security;

namespace Azos.Web.Shipping.Shippo
{
  public class ShippoConnectionParameters : ShippingConnectionParameters
  {
    #region ctor

      public ShippoConnectionParameters() : base() { }

      public ShippoConnectionParameters(IConfigSectionNode node) : base(node) { }

      public ShippoConnectionParameters(string connStr, string format = Configuration.CONFIG_LACONIC_FORMAT)
        : base(connStr, format) {}

    #endregion

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      var privateToken = node.AttrByName("private-token").ValueAsString();
      if (privateToken.IsNullOrWhiteSpace())
      {
        User = User.Fake;
        return;
      }

      var publicToken = node.AttrByName("public-token").ValueAsString();
      if (publicToken.IsNullOrWhiteSpace())
      {
        User = User.Fake;
        return;
      }

      var cred = new ShippoCredentials(privateToken, publicToken);
      var token = new AuthenticationToken(ShippoSystem.SHIPPO_REALM, null);
      User = new User(cred, token, null, Rights.None);
    }
  }
}
