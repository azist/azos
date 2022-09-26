/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

using Azos.Conf;

namespace Azos.Wave.Handlers
{
  /// <summary>
  /// This handler serves the embedded content of Azos.Wave library
  /// </summary>
  public class StockContentSiteHandler : EmbeddedSiteHandler
  {
    public StockContentSiteHandler(WorkHandler director, string name, int order, WorkMatch match)
                          : base(director, name, order, match){}


    public StockContentSiteHandler(WorkHandler director, IConfigSectionNode confNode)
                          : base(director, confNode) {}


    public override string RootResourcePath => "Azos.Wave.Templatization.StockContent.Embedded";

    public override bool SupportsEnvironmentBranching => false;

    protected override IEnumerable<EmbeddedSiteHandler.IAction> GetActions()
    {
      yield break;
    }
  }
}
