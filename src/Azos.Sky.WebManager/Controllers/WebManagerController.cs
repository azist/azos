/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
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
