using Azos;

using Azos.Wave;
using Azos.Wave.Mvc;
using Azos.Sky.Metabase;

namespace Azos.Sky.WebManager.Controllers
{
  /// <summary>
  /// Provides base for WebManager controllers
  /// </summary>
  public abstract class WebManagerController : Controller
  {
    public new ISkyApplication App => base.App.AsSky();
    public Metabank Metabase => App.GetMetabase();
    internal Localizer Localizer => Localizer.Of(App);
  }
}
