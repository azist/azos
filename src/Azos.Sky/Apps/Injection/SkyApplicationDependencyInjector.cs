using System;
using System.Collections.Generic;
using System.Text;
using Azos.Conf;

using Azos.Sky;

namespace Azos.Apps.Injection
{
  /// <summary>
  /// Implements sky app dependency injection services
  /// </summary>
  public class SkyApplicationDependencyInjector : ApplicationDependencyInjector
  {
    public SkyApplicationDependencyInjector(IApplication app) : base(app)
    {
    }

    public ISkyApplication SkyApp => App.AsSky();

    /// <summary>
    /// Enumerates app injectable roots (root application chassis objects).
    /// This method is usually used by [Inject]-derived attributes for defaults
    /// </summary>
    public virtual IEnumerable<object> GetApplicationRoots()
    {
      yield return App;
      yield return SkyApp;
      yield return App.Log;
      yield return App.DataStore;
      yield return App.Instrumentation;
      yield return App.Glue;
      yield return App.GetServiceClientHub();
      yield return SkyApp.Metabase;
      yield return SkyApp.GdidProvider;
      yield return SkyApp.ProcessManager;
      yield return SkyApp.LockManager;
      yield return SkyApp.DynamicHostManager;

      foreach (var root in base.GetApplicationRoots())//the rest, some already included above
        yield return root;
    }
  }
}
