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

    /// <summary>
    /// Captures a sequenced registry of filters and current position of processing
    /// </summary>
    public struct CallChain
    {
      /// <summary>
      /// Starts new filter call chain starting from zero
      /// </summary>
      public CallChain(IOrderedRegistry<WorkFilter> filters)
      {
        filters.NonNull(nameof(filters));
        Filters = filters;
        CurrentIndex = 0;
      }

      /// <summary>
      /// Advances the chain to the next caller index
      /// </summary>
      public CallChain(CallChain current)
      {
        current.Assigned.IsTrue("assigned current");
        Filters = current.Filters;
        CurrentIndex = current.CurrentIndex + 1;
      }

      public readonly IOrderedRegistry<WorkFilter> Filters;
      public readonly int CurrentIndex;
      public bool Assigned => Filters != null;

      /// <summary> Get current filter or null if not assigned or EOF </summary>
      public WorkFilter Current => Filters?[CurrentIndex];
    }



    /// <summary>
    /// Registers matches declared in config. Throws error if registry already contains a match with a duplicate name
    /// </summary>
    public static void MakeAndRegisterFromConfig(WorkHandler handler, OrderedRegistry<WorkFilter> registry, IConfigSectionNode confNode)
    {
      foreach (var cn in confNode.NonNull(nameof(confNode)).ChildrenNamed(CONFIG_FILTER_SECTION))
      {
        var filter = FactoryUtils.MakeDirectedComponent<WorkFilter>(handler.NonNull(nameof(handler)), cn, extraArgs: new object[] { cn });
        if (!registry.NonNull(nameof(registry)).Register(filter))
        {
          throw new WaveException(StringConsts.CONFIG_DUPLICATE_FILTER_NAME_ERROR.Args(filter.Name, handler.Name));
        }
      }
    }


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
    /// <param name="callChain">
    /// Filter call chain that should be passed into <see cref="InvokeNextWorkerAsync(WorkContext, CallChain)"/>
    /// </param>
    public async Task FilterWorkAsync(WorkContext work, CallChain callChain)
    {
      try
      {
        await DoFilterWorkAsync(work, callChain).ConfigureAwait(false);
      }
      catch(FilterPipelineException){ throw; }//#783 20230218 DKh
      catch(Exception error) //wrap error in fpe
      {
        work.LastError = error;
        throw new FilterPipelineException(this, callChain, error);
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
    protected async Task InvokeNextWorkerAsync(WorkContext work, CallChain callChain)
    {
      if (work == null) return;
      if (work.Aborted || work.Handled) return;

      var next = new CallChain(callChain);//advances to the next worker
      var nextFilter = next.Current;
      if (nextFilter != null)
      {
        //if there is a next filter, then it may elect to handle work by itself or
        //within its call chain
        await nextFilter.FilterWorkAsync(work, next).ConfigureAwait(false);
      }
      else
      {
        //if there is no NEXT filter, we need to handle the work to handler that owns this filter
        await this.Handler.HandleWorkAsync(work).ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Override to filter the work - i.e. extract some security name from cookies and check access, turn exception in error page etc.
    /// Note: This method is re-entrant by multiple threads. Do not forget to call InvokeNextWorker() to continue request processing, otherwise the work will
    ///  not be handled (which may be a desired behavior)
    /// </summary>
    /// <param name="work">Work context</param>
    /// <param name="callChain">
    /// Filter call chain to pass along
    /// </param>
    protected abstract Task DoFilterWorkAsync(WorkContext work, CallChain callChain);
  }
}
