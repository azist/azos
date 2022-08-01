/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;

namespace Azos.Wave
{
  /// <summary>
  /// Represents a base for all work filters. Unlike handlers, filters do not necessarily handle work rather augment the work context
  /// </summary>
  public abstract class WorkFilter : ApplicationComponent<WorkHandler>, INamed, IOrdered
  {
    public const string CONFIG_FILTER_SECTION = "filter";

    protected WorkFilter(WorkHandler director, string name, int order) : base(director)
    {
      m_Name = name.Default("{0}({1})".Args(GetType().FullName, Guid.NewGuid()));
      m_Order = order;
    }

    protected WorkFilter(WorkHandler director, IConfigSectionNode confNode) : base(director)
    {
      confNode.NonEmpty(nameof(confNode));

      ConfigAttribute.Apply(this, confNode);

      m_Name = confNode.AttrByName(Configuration.CONFIG_NAME_ATTR).Value.Default("{0}({1})".Args(GetType().FullName, Guid.NewGuid()));
      m_Order = confNode.AttrByName(Configuration.CONFIG_ORDER_ATTR).ValueAsInt(0);
    }

    private readonly string m_Name;
    private readonly int m_Order;

    public override string ComponentLogTopic => CoreConsts.WAVE_TOPIC;

    /// <summary>
    /// Returns the filter instance name
    /// </summary>
    public string Name => m_Name;

    /// <summary>
    /// Returns the filter order in filter registry. Order is used for URI pattern matching
    /// </summary>
    public int Order => m_Order;


    /// <summary>
    /// Returns the server that this filter works under
    /// </summary>
    public WorkHandler Handler => ComponentDirector;

    /// <summary>
    /// Returns the server that this filter works under
    /// </summary>
    public WaveServer Server =>  Handler.Server;




    /// <summary>
    /// Override to filter the work - i.e. extract some security name from cookies and check access, turn exception in error page etc.
    /// Note: This method is re-entrant by multiple threads
    /// </summary>
    /// <param name="work">Work context</param>
    /// <param name="filters">
    /// The filters that participated in a call.
    /// Note the Dipatcher.Filters may yield different results as it may change with time, whereas this parameter captures all filters during the call start
    /// </param>
    /// <param name="thisFilterIndex">
    /// The index of THIS filter in filters
    /// </param>
    public async Task FilterWorkAsync(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex)
    {
      try
      {
        await DoFilterWork(work, filters, thisFilterIndex).ConfigureAwait(false);
      }
      catch(Exception error)
      {
        work.LastError = error;
        throw new FilterPipelineException(this, error);
      }
    }


    public override string ToString()
    {
      return "{0}('{1}',#{2})".Args(GetType().FullName, m_Name, m_Order);
    }


    /// <summary>
    /// Invokes next processing body be it the next filter or handler (when all filters are iterated through).
    /// The filter implementors must call this method to pass WorkContext processing along the line.
    /// Does nothing if work is Aborted or Handled
    /// </summary>
    protected void InvokeNextWorker(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex)
    {
        if (work==null) return;
        if (work.Aborted || work.Handled) return;

        var idxNext = thisFilterIndex+1;
        if (idxNext<filters.Count)
        {
          filters[idxNext].FilterWork(work, filters, idxNext);
        }
        else
        {
        WorkHandler handler = m_Handler;

        if (handler!=null)//if we are under Handler already then call the handler directly
          handler.HandleWork(work);
        else //if we are under dispatcher then call dipatcher to locate and invoke the matching handler
          m_Dispatcher.InvokeHandler(work, out handler);
        }
    }

    /// <summary>
    /// Override to filter the work - i.e. extract some security name from cookies and check access, turn exception in error page etc.
    /// Note: This method is re-entrant by multiple threads. Do not forget to call InvokeNextWorker() to continue request processing, otherwise the work will
    ///  not be handled (which may be a desired behavior)
    /// </summary>
    /// <param name="work">Work context</param>
    /// <param name="filters">
    /// The filters that participated in a call.
    /// Note the Dipatcher.Filters may yield different results as it may change with time, whereas this parameter captures all filters during the call start
    /// </param>
    /// <param name="thisFilterIndex">
    /// The index of THIS filter in filters
    /// </param>
    protected abstract Task DoFilterWorkAsync(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex);
  }
}
