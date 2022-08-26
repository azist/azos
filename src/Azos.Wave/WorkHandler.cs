/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using System.Threading.Tasks;

namespace Azos.Wave
{
  /// <summary>
  /// Represents a base for all work handlers. Handlers are final work execution destination
  /// </summary>
  public abstract class WorkHandler : ApplicationComponent, INamed, IOrdered
  {
    public const string CONFIG_HANDLER_SECTION = "handler";

    protected WorkHandler(WorkHandler director, string name, int order, WorkMatch match = null) : base(director)
    {
      m_Name = name.Default("{0}({1})".Args(GetType().FullName, Guid.NewGuid()));
      m_Order = order;

      if (match != null)
      {
        m_Matches.Register(match);
      }
    }

    internal WorkHandler(WaveServer director, IConfigSectionNode confNode) : base(director) => ctor(confNode);
    protected WorkHandler(WorkHandler director, IConfigSectionNode confNode) : base(director) => ctor(confNode);

    private void ctor(IConfigSectionNode confNode)
    {
      confNode.NonEmpty(nameof(confNode));

      ConfigAttribute.Apply(this, confNode);

      m_Name = confNode.AttrByName(Configuration.CONFIG_NAME_ATTR).Value.Default("{0}({1})".Args(GetType().FullName, Guid.NewGuid()));
      m_Order = confNode.AttrByName(Configuration.CONFIG_ORDER_ATTR).ValueAsInt(0);

      WorkFilter.MakeAndRegisterFromConfig(this, m_Filters, confNode);
      WorkMatch.MakeAndRegisterFromConfig(m_Matches, confNode, ToString());
    }

    protected override void Destructor()
    {
      base.Destructor();
      foreach(var filter in Filters) filter.Dispose();
    }


    private string m_Name;
    private int m_Order;
    private OrderedRegistry<WorkMatch> m_Matches = new OrderedRegistry<WorkMatch>();
    private OrderedRegistry<WorkFilter> m_Filters = new OrderedRegistry<WorkFilter>();


    public override string ComponentLogTopic => CoreConsts.WAVE_TOPIC;
    public override string ComponentCommonName => Name;


    /// <summary>
    /// Returns the paent handler that this handler works under or null if this is a
    /// root handler which works directly under server
    /// </summary>
    public WorkHandler ParentHandler => ComponentDirector as WorkHandler;


    /// <summary>
    /// Returns the server that this handler works under
    /// </summary>
    public WaveServer Server => (ParentHandler?.Server) ?? (WaveServer)ComponentDirector;

    /// <summary>
    /// Returns the handler instance name
    /// </summary>
    public string Name => m_Name;

    /// <summary>
    /// Returns the handler order in handler registry. Order is used for URI pattern matching
    /// </summary>
    public int Order => m_Order;

    /// <summary>
    /// Returns matches used by this handler. May change the registry at runtime (inject/remove matches)
    /// </summary>
    public OrderedRegistry<WorkMatch> Matches => m_Matches;

    /// <summary>
    /// Returns ordered registry of filters
    /// </summary>
    public IRegistry<WorkFilter> Filters => m_Filters;

    /// <summary>
    /// Registers filter and returns true if the named instance has not been registered yet
    /// Note: it is possible to call this method on active server that is - inject filters while serving requests
    /// </summary>
    public bool RegisterFilter(WorkFilter filter)
    {
      if (filter == null) return false;
      if (filter.Handler != this)
        throw new WaveException(StringConsts.WRONG_HANDLER_FILTER_REGISTRATION_ERROR.Args(filter));

      return m_Filters.Register(filter);
    }

    /// <summary>
    /// Unregisters filter and returns true if the named instance has been removed
    /// Note: it is possible to call this method on active server that is - remove filters while serving requests
    /// </summary>
    public bool UnRegisterFilter(WorkFilter filter)
    {
      if (filter == null) return false;
      if (filter.Handler != this)
        throw new WaveException(StringConsts.WRONG_HANDLER_FILTER_UNREGISTRATION_ERROR.Args(filter));

      return m_Filters.Unregister(filter);
    }

    /// <summary>
    /// Unregisters filter and returns true if the named instance has been removed
    /// Note: it is possible to call this method on active server that is - remove filters while serving requests
    /// </summary>
    public bool UnRegisterFilter(string name)
    {
      if (name.IsNullOrWhiteSpace()) return false;
      return m_Filters.Unregister(name);
    }


    /// <summary>
    /// Handles the work -  first invokes all filters then calls HandleWork to do actual processing
    /// Note: This method is re-entrant by multiple callflows/threads
    /// </summary>
    public async Task FilterAndHandleWorkAsync(WorkContext work)
    {
      var chain = new WorkFilter.CallChain(m_Filters);//captures context

      var filter = chain.Current;
      if (filter != null)
      {
        //if there is a filter it MAY elect to keep processing and
        //finally handle work from within its callchain
        await filter.FilterWorkAsync(work, chain).ConfigureAwait(false);
      }
      else
      {
        //if no filter exists then we need to handle work explicitly here
        await HandleWorkAsync(work).ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Handles the work - i.e. renders a template page or calls MVC controller method.
    /// This method does not pass through handler's filters
    /// Note: This method is re-entrant by multiple callflows/threads
    /// </summary>
    public async Task HandleWorkAsync(WorkContext work)
    {
      try
      {
        work.m_Handler = this;
        await DoHandleWorkAsync(work).ConfigureAwait(false);
      }
      finally
      {
        work.m_Handled = true;
      }
    }

    public override string ToString() => $"{GetType().DisplayNameWithExpandedGenericArgs()}(`{m_Name}`, #{m_Order})";

    /// <summary>
    /// Returns true when the particular work request matches the pattern match of this handler.
    /// Also sets the WorkHandler's MatchedVars property filled with matched values
    /// </summary>
    public virtual bool MakeMatch(WorkContext work)
    {
      foreach(var match in m_Matches.OrderedValues)
      {
        var matched = match.Make(work);
        if (matched != null)
        {
          work.___SetWorkMatch(match, matched);
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Override to handle the work - i.e. render a template page or call MVC controller method.
    /// Note: This method is re-entrant by multiple threads
    /// </summary>
    protected abstract Task DoHandleWorkAsync(WorkContext work);
  }
}
