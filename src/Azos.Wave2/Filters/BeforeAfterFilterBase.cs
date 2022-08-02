/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Serialization.JSON;
using Azos.Collections;
using System.Threading.Tasks;

namespace Azos.Wave.Filters
{
  /// <summary>
  /// Provides base for filters that have before/after semantics
  /// </summary>
  public abstract class BeforeAfterFilterBase : WorkFilter
  {
    #region CONSTS
    public const string CONFIG_BEFORE_SECTION = "before";
    public const string CONFIG_AFTER_SECTION = "after";
    #endregion

    #region .ctor
    public BeforeAfterFilterBase(WorkHandler handler, string name, int order) : base(handler, name, order)
    {
    }

    public BeforeAfterFilterBase(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode)
    {
      WorkMatch.MakeAndRegisterFromConfig(m_BeforeMatches, confNode[CONFIG_BEFORE_SECTION], "{0} before".Args(GetType().DisplayNameWithExpandedGenericArgs()));
      WorkMatch.MakeAndRegisterFromConfig(m_AfterMatches, confNode[CONFIG_AFTER_SECTION], "{0} after".Args(GetType().DisplayNameWithExpandedGenericArgs()));
    }
    #endregion

    #region Fields
    private OrderedRegistry<WorkMatch> m_BeforeMatches = new OrderedRegistry<WorkMatch>();
    private OrderedRegistry<WorkMatch> m_AfterMatches = new OrderedRegistry<WorkMatch>();
    #endregion

    #region Properties
    /// <summary>
    /// Returns matches used by the handler to determine whether match should be made before the work processing
    /// </summary>
    public OrderedRegistry<WorkMatch> BeforeMatches => m_BeforeMatches;

    /// <summary>
    /// Returns matches used by the handler to determine whether match should be made before the work processing
    /// </summary>
    public OrderedRegistry<WorkMatch> AfterMatches => m_AfterMatches;
    #endregion

    #region Protected
    protected sealed override async Task DoFilterWorkAsync(WorkContext work, CallChain callChain)
    {
      if (m_BeforeMatches.Count>0)
      {
        foreach(var match in m_BeforeMatches.OrderedValues)
        {
          var matched = match.Make(work);
          if (matched != null)
          {
            await DoBeforeWorkAsync(work, matched).ConfigureAwait(false);
            break;
          }
        }
      }

      await this.InvokeNextWorkerAsync(work, callChain);

      if (m_AfterMatches.Count>0)
      {
        foreach(var match in m_AfterMatches.OrderedValues)
        {
          var matched = match.Make(work);
          if (matched != null)
          {
            await DoAfterWorkAsync(work, matched).ConfigureAwait(false);
            break;
          }
        }
      }
    }

    /// <summary>
    /// Override to do the work when one of the BeforeMatches was matched
    /// </summary>
    protected abstract Task DoBeforeWorkAsync(WorkContext work, JsonDataMap matched);

    /// <summary>
    /// Override to do the work when one of the AfterMatches was matched
    /// </summary>
    protected abstract Task DoAfterWorkAsync(WorkContext work, JsonDataMap matched);

    #endregion
  }
}
