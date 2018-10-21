
using Azos.Conf;
using Azos.Security;

namespace Azos.Web.Pay.Stripe
{
  public class StripeConnectionParameters: ConnectionParameters
  {
    public const string STRIPE_REALM = "stripe";

    public const string CONFIG_EMAIL_ATTR = "email";
    public const string CONFIG_SECRETKEY_ATTR = "secret-key";
    public const string CONFIG_PUBLISHABLEKEY_ATTR = "publishable-key";

    public StripeConnectionParameters(): base() {}
    public StripeConnectionParameters(IConfigSectionNode node): base(node) {}
    public StripeConnectionParameters(string connectString, string format = Configuration.CONFIG_LACONIC_FORMAT)
      : base(connectString, format) {}

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      var email = node.AttrByName(CONFIG_EMAIL_ATTR).Value;
      var secretKey = node.AttrByName(CONFIG_SECRETKEY_ATTR).Value;
      var publishableKey = node.AttrByName(CONFIG_PUBLISHABLEKEY_ATTR).Value;

      var cred = new StripeCredentials(email, secretKey, publishableKey);
      var at = new AuthenticationToken(STRIPE_REALM, publishableKey);

      User = new User(cred, at, UserStatus.User, publishableKey, publishableKey, Rights.None);
    }

  } //StripeSystemSessionParameters

}
