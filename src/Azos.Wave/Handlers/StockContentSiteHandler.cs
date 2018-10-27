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
    public StockContentSiteHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match)
                          : base(dispatcher, name, order, match){}


    public StockContentSiteHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode)
                          : base(dispatcher, confNode) {}


    public override string RootResourcePath
    {
      get { return "Azos.Wave.Templatization.StockContent.Embedded"; }
    }

    public override bool SupportsEnvironmentBranching
    {
      get { return false; }
    }

    protected override IEnumerable<EmbeddedSiteHandler.IAction> GetActions()
    {
      yield break;
    }
  }


}
