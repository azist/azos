/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Collections;
using Azos.Data;
using System.Threading.Tasks;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Upon match, redirects client to the specified URL resource. Specify matches with 'redirect-url' var
  /// </summary>
  public class RedirectFilter : WorkFilter
  {
    #region CONSTS
    public const string VAR_REDIRECT_URL = "redirect-url";
    public const string VAR_REDIRECT_TARGET = "redirect-target";
    #endregion

    #region .ctor
    public RedirectFilter(WorkHandler handler, string name, int order) : base(handler, name, order) {}
    public RedirectFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode)
    {
      WorkMatch.MakeAndRegisterFromConfig(m_RedirectMatches, confNode, GetType().Name);
    }

    #endregion

    #region Fields
    private OrderedRegistry<WorkMatch> m_RedirectMatches = new OrderedRegistry<WorkMatch>();
    #endregion

    #region Properties
    /// <summary>
    /// Returns matches used by the filter to determine whether redirect should be issued
    /// </summary>
    public OrderedRegistry<WorkMatch> RedirectMatches => m_RedirectMatches;
    #endregion

    #region Protected
    protected override async Task DoFilterWorkAsync(WorkContext work, CallChain callChain)
    {
      foreach(var match in m_RedirectMatches.OrderedValues)
      {
        var matched = match.Make(work);
        if (matched==null) continue;

        var url = matched[VAR_REDIRECT_URL].AsString();
        if (url.IsNotNullOrWhiteSpace())
        {
          var target = matched[VAR_REDIRECT_TARGET].AsString();
          if (target.IsNotNullOrWhiteSpace())
          {
            var partsA = url.Split('#');
            var parts = partsA[0].Split('?');
            var query = parts.Length > 1 ? parts[0] + "&" : string.Empty;
            url = "{0}?{1}{2}={3}{4}".Args(parts[0], query,
              target, Uri.EscapeDataString(work.Request.Url),
              partsA.Length > 1 ? "#" + partsA[1] : string.Empty);
          }

          work.Response.RedirectAndAbort(url);
          return;
        }
      }

      await this.InvokeNextWorkerAsync(work, callChain).ConfigureAwait(false);
    }
    #endregion
  }
}
