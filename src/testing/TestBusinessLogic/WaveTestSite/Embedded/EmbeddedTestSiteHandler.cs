/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

using Azos;
using Azos.Conf;
using Azos.Data;
using Azos.Web;
using Azos.Wave;
using Azos.Wave.Handlers;
using System.Threading.Tasks;

namespace WaveTestSite.Embedded
{
  /// <summary>
  /// This handler serves the particular embedded site
  /// </summary>
  public class EmbeddedTestSiteHandler : EmbeddedSiteHandler
  {
    public EmbeddedTestSiteHandler(WorkHandler director, string name, int order, WorkMatch match)
                          : base(director, name, order, match){}


    public EmbeddedTestSiteHandler(WorkHandler director, IConfigSectionNode confNode)
                          : base(director, confNode) {}


    public override string RootResourcePath
    {
      get { return "WaveTestSite.Embedded"; }
    }

    protected override IEnumerable<EmbeddedSiteHandler.IAction> GetActions()
    {
      yield return new CountAction();
    }
  }

  /// <summary>
  /// Counts from one int to another
  /// </summary>
  public class CountAction : EmbeddedSiteHandler.IAction
  {
    public string Name{ get { return "Count"; } }

    public async Task PerformAsync(WorkContext context)
    {
      var from = context.Request.Query["from"].AsInt(1);
      var to   = context.Request.Query["to"].AsInt(10);

      if (to-from>1000) to = from + 1000;//limit so no infinite loop possible

      context.Response.ContentType = ContentType.TEXT;
      for(var i=from;i<=to;i++)
        await context.Response.WriteLineAsync("{0} times and counting".Args(i));
    }
  }


}
