
using System;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Web.Pay
{
  /// <summary>
  /// Represents a starter that launches payment systems on startup
  /// </summary>
  public sealed class PaySystemStarter : IApplicationStarter
  {
    [Config]
    public string Name { get; set; }

    [Config]
    public bool ApplicationStartBreakOnException { get; set; }

    public void ApplicationStartBeforeInit(IApplication application) { }

    public void ApplicationStartAfterInit(IApplication application)
    {
      PaySystem.AutoStartSystems();
    }

    public void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }
  }

}
