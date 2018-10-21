
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Wave;
using Azos.Wave.Handlers;
using Azos.Environment;

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
