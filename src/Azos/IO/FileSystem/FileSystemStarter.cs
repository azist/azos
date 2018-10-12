
using Azos.Apps;
using Azos.Conf;

namespace Azos.IO.FileSystem
{
  /// <summary>
  /// Represents a starter that launches file systems on startup
  /// </summary>
  public sealed class FileSystemStarter : IApplicationStarter
  {
    [Config]
    public string Name { get; set; }

    [Config]
    public bool ApplicationStartBreakOnException { get; set; }

    public void ApplicationStartBeforeInit(IApplication application) { }

    public void ApplicationStartAfterInit(IApplication application)
    {
      FileSystem.AutoStartSystems();
    }

    public void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }
  }
}
