
using Azos.Conf;
using Azos.Security;

namespace Azos.Web.Shipping
{
  /// <summary>
  /// Parameters used for connect to API
  /// </summary>
  public class ShippingConnectionParameters : Collections.INamed, IConfigurable
  {
    #region Static

      public static TParams Make<TParams>(IConfigSectionNode node)
        where TParams: ShippingConnectionParameters
      {
        return FactoryUtils.MakeAndConfigure<TParams>(node, typeof(TParams), args: new object[] { node });
      }

      public static TParams Make<TParams>(string connStr, string format = Configuration.CONFIG_LACONIC_FORMAT)
        where TParams: ShippingConnectionParameters
      {
        var cfg = Configuration.ProviderLoadFromString(connStr, format).Root;
        return Make<TParams>(cfg);
      }

    #endregion

    #region ctor

    public ShippingConnectionParameters() {}

      public ShippingConnectionParameters(IConfigSectionNode node)
      {
        Configure(node);
      }

      public ShippingConnectionParameters(string connStr, string format = Configuration.CONFIG_LACONIC_FORMAT)
      {
        var conf = Configuration.ProviderLoadFromString(connStr, format).Root;
        Configure(conf);
      }

	  #endregion

    [Config]
    public string Name { get; set; }

    public User User { get; set; }


    public virtual void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }
  }
}
