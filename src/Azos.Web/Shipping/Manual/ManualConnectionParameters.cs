
using Azos.Conf;
using Azos.Security;

namespace Azos.Web.Shipping.Manual
{
  public class ManualConnectionParameters : ShippingConnectionParameters
  {
    #region ctor

      public ManualConnectionParameters() : base() { }

      public ManualConnectionParameters(IConfigSectionNode node) : base(node) { }

      public ManualConnectionParameters(string connStr, string format = Configuration.CONFIG_LACONIC_FORMAT)
        : base(connStr, format) {}

    #endregion

    public override void Configure(IConfigSectionNode node)
    {
      base.Configure(node);

      var cred = new ManualCredentials();
      var token = new AuthenticationToken(ManualSystem.MANUAL_REALM, null);
      User = new User(cred, token, null, Rights.None);
    }
  }
}
