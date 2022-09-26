/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System.Threading.Tasks;

namespace Azos.Wave.Mvc
{
  /// <summary>
  /// Decorates entities that get returned by MVC actions and can get executed to generate some result action (command pattern)
  /// </summary>
  public interface IActionResult
  {
     Task ExecuteAsync(Controller controller, WorkContext work);
  }
}
