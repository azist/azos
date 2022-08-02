/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Collections;
using Azos.Security;
using System.Threading.Tasks;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Checks permissions before doing work
  /// </summary>
  public sealed class SecurityFilter : WorkFilter
  {
    public const string CONFIG_BYPASS_SECTION = "bypass";

    #region .ctor
    public SecurityFilter(WorkHandler handler, string name, int order) : base(handler, name, order){}
    public SecurityFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode) { ctor(confNode); }

    private void ctor(IConfigSectionNode confNode)
    {
      var permsNode = confNode[Permission.CONFIG_PERMISSIONS_SECTION];
      if (permsNode.Exists)
      {
        m_Permissions = Permission.MultipleFromConf(permsNode);
      }

      WorkMatch.MakeAndRegisterFromConfig(m_BypassMatches, confNode[CONFIG_BYPASS_SECTION], GetType().Name);
    }

    #endregion

    #region Fields
    private IEnumerable<Permission> m_Permissions;
    private OrderedRegistry<WorkMatch> m_BypassMatches = new OrderedRegistry<WorkMatch>();
    #endregion

    #region Properties
    /// <summary>
    /// Gets/sets permissions that get checked by this filter instance
    /// </summary>
    public IEnumerable<Permission> Permissions
    {
      get{return m_Permissions;}
      set{m_Permissions = value;}
    }
    #endregion

    #region Protected
    protected override async Task DoFilterWorkAsync(WorkContext work, CallChain callChain)
    {
      if (m_Permissions!=null && m_Permissions.Any())
      {
        if (!m_BypassMatches.Any(match => match.Make(work)!=null))
        {
          work.NeedsSession();
          Permission.AuthorizeAndGuardAction(App.SecurityManager, m_Permissions, "{0}({1})".Args(GetType().FullName, this.Name), work.Session);
        }
      }

      await this.InvokeNextWorkerAsync(work, callChain).ConfigureAwait(false);
    }
    #endregion
  }
}
