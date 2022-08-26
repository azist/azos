/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;

using Azos.Conf;

namespace Azos.Wave.Handlers
{
  /// <summary>
  /// Implements handler that does nothing
  /// </summary>
  public sealed class NOPHandler : WorkHandler
  {
    public NOPHandler(WorkHandler director, string name, int order, WorkMatch match = null)
                        : base(director, name, order, match){ }

    public NOPHandler(WorkHandler director, IConfigSectionNode confNode)
                        : base(director, confNode){ }

    protected override Task DoHandleWorkAsync(WorkContext work)
      => Task.CompletedTask;
  }
}
