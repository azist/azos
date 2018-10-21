
using Azos.Collections;
using Azos.Conf;
using Azos.Security;

namespace Azos.Web.Pay
{
  public class ConnectionParameters: INamed, IConfigurable
  {
    #region Static

 /*   // 20170922  Eibr + DKH  Remove old unused code
      private static Registry<IPaySystemHost> s_PaySystemHosts = new Registry<IPaySystemHost>();

      public static IRegistry<IPaySystemHost> PaySystemHosts { get { return s_PaySystemHosts;} }

      public static IPaySystemHost GetSystemHost(string name)
      {
        if (s_PaySystemHosts.ContainsName(name)) return s_PaySystemHosts[name];

        throw new NotImplementedException();
      }
*/

      public static TParams Make<TParams>(IConfigSectionNode node) where TParams: ConnectionParameters
      {
        return FactoryUtils.MakeAndConfigure<TParams>(node, typeof(TParams), args: new object[] {node});
      }

      public static TParams Make<TParams>(string connStr, string format = Configuration.CONFIG_LACONIC_FORMAT)
        where TParams: ConnectionParameters
      {
        var cfg = Configuration.ProviderLoadFromString(connStr, format).Root;
        return Make<TParams>(cfg);
      }

    #endregion

    #region ctor

      public ConnectionParameters() {}

      public ConnectionParameters(IConfigSectionNode node) { Configure(node); }

      public ConnectionParameters(string connStr, string format = Configuration.CONFIG_LACONIC_FORMAT)
      {
        var conf = Configuration.ProviderLoadFromString(connStr, format).Root;
      }

    #endregion

    #region Properties

      [Config] public string Name { get; set; }

      public User User {get; set;}

    #endregion

    #region Protected

      public virtual void Configure(IConfigSectionNode node)
      {
        ConfigAttribute.Apply(this, node);
      }

    #endregion
  } //PayConnectionParameters

}
