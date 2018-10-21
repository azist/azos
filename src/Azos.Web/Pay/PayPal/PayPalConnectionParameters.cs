
using Azos.Conf;
using Azos.Security;

namespace Azos.Web.Pay.PayPal
{
  public class PayPalConnectionParameters : ConnectionParameters
  {
    #region .ctor
    public PayPalConnectionParameters() : base() { }
    public PayPalConnectionParameters(IConfigSectionNode node) : base(node) { }
    public PayPalConnectionParameters(string connectionString, string format = Configuration.CONFIG_LACONIC_FORMAT)
        : base(connectionString, format) { }
    #endregion

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      User = User.Fake;

      var clientID = node.AttrByName("client-id").Value;
      if (clientID.IsNullOrWhiteSpace()) return;
      var clientSecret = node.AttrByName("client-secret").Value;
      if (clientSecret.IsNullOrWhiteSpace()) return;

      User = new User(new PayPalCredentials(clientID, clientSecret), new AuthenticationToken(PayPalSystem.PAYPAL_REALM, null), string.Empty, Rights.None);
    }
  }
}
