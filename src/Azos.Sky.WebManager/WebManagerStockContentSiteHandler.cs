/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;
using Azos.Wave;
using Azos.Wave.Handlers;

namespace Azos.Sky.WebManager
{
  /// <summary>
  /// This handler serves the embedded content of Sky Web Manager site
  /// </summary>
  public class WebManagerStockContentSiteHandler : StockContentSiteHandler
  {
    public WebManagerStockContentSiteHandler(WorkHandler director, string name, int order, WorkMatch match)
                          : base(director, name, order, match){}


    public WebManagerStockContentSiteHandler(WorkHandler director, IConfigSectionNode confNode)
                          : base(director, confNode) {}


    public override string RootResourcePath
    {
      get { return "Azos.Sky.WebManager.Site"; }
    }
  }
}
