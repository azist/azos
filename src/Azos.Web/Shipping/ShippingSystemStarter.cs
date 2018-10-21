
using Azos.Apps;
using Azos.Conf;

namespace Azos.Web.Shipping
{
  /// <summary>
  /// Represents a starter that launches shipping systems on startup
  /// </summary>
  public sealed class ShippingSystemStarter : IApplicationStarter
  {
    [Config]
    public string Name { get; set; }

    [Config]
    public bool ApplicationStartBreakOnException { get; set; }

    public void ApplicationStartBeforeInit(IApplication application)
    {
    }

    public void ApplicationStartAfterInit(IApplication application)
    {
      ShippingSystem.AutoStartSystems();
    }

    public void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }
  }
}
