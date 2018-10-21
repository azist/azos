
using Azos.Apps;
using Azos.Conf;

namespace Azos.Web.Social
{
  public sealed class SocialNetworkStarter: IApplicationStarter
  {
    [Config]
    public string Name { get; set; }

    [Config]
    public bool ApplicationStartBreakOnException { get; set; }

    public void ApplicationStartBeforeInit(IApplication application) {}

    public void ApplicationStartAfterInit(IApplication application)
    {
      SocialNetwork.AutoStartNetworks();
    }

    public void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }
  }
}
