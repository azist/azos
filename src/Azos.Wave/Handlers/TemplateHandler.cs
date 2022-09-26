/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Threading.Tasks;

using Azos.Conf;
using Azos.Wave.Templatization;

namespace Azos.Wave.Handlers
{
  /// <summary>
  /// Implements handler that serves WaveTemplates
  /// </summary>
  public class TemplateHandler : TypeLookupHandler<WaveTemplate>
  {
    protected TemplateHandler(WorkHandler director, string name, int order, WorkMatch match)
                      : base(director, name, order, match){ }
    protected TemplateHandler(WorkHandler director, IConfigSectionNode confNode)
                      : base(director, confNode){ }

    protected override Task DoTargetWorkAsync(WaveTemplate target, WorkContext work)
    {
      target.BindGlobalContexts(work);
      target.Render(new ResponseRenderingTarget(work), null);
      return Task.CompletedTask;
    }
  }
}
