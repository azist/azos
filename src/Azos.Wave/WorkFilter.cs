/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Collections;
using Azos.IO.Net.Gate;

namespace Azos.Wave
{

  /// <summary>
  /// Represents a base for all work filters. Unlike handlers, filters do not necessarily handle work rather augment the work context
  /// </summary>
  public abstract class WorkFilter : ApplicationComponent, INamed, IOrdered
  {
      public const string CONFIG_FILTER_SECTION = "filter";

      protected WorkFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher)
      {
        if (name.IsNullOrWhiteSpace()||dispatcher==null)
         throw new WaveException(StringConsts.ARGUMENT_ERROR + GetType().FullName+".ctor(dispatcher|name==null|empty)");

        m_Name = name;
        m_Dispatcher = dispatcher;
        m_Server = dispatcher.ComponentDirector;
        m_Order = order;
      }

      protected WorkFilter(WorkHandler handler, string name, int order) : this(handler.NonNull(name: ".ctor(handler==null)").Dispatcher, name, order)
      {
        m_Handler = handler;
        this.__setComponentDirector(handler);
      }


      protected WorkFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher)
      {
        if (confNode==null||dispatcher==null)
         throw new WaveException(StringConsts.ARGUMENT_ERROR + GetType().FullName+".ctor(dispatcher|confNode==null|empty)");

        m_Dispatcher = dispatcher;
        m_Server = dispatcher.ComponentDirector;
        m_Name = confNode.AttrByName(Configuration.CONFIG_NAME_ATTR).Value;
        m_Order = confNode.AttrByName(Configuration.CONFIG_ORDER_ATTR).ValueAsInt(0);

        if (m_Name.IsNullOrWhiteSpace())
         throw new WaveException(StringConsts.ARGUMENT_ERROR + GetType().FullName+".ctor(confNode$name==null|empty)");
      }

      protected WorkFilter(WorkHandler handler, IConfigSectionNode confNode) : this(handler.NonNull(name: ".ctor(handler==null)").Dispatcher, confNode)
      {
        m_Handler = handler;
        this.__setComponentDirector(handler);
      }

      private string m_Name;
      private int m_Order;
      private WorkDispatcher m_Dispatcher;
      private WaveServer m_Server;
      private WorkHandler m_Handler;

      /// <summary>
      /// Returns the filter instance name
      /// </summary>
      public string Name { get{ return m_Name;}}

      /// <summary>
      /// Returns the filter order in filter registry. Order is used for URI pattern matching
      /// </summary>
      public int Order { get{ return m_Order;}}


      /// <summary>
      /// Returns the server that this filter works under
      /// </summary>
      public WaveServer Server { get{ return m_Server;}}

      /// <summary>
      /// Returns the dispatcher that this filter works under
      /// </summary>
      public WorkDispatcher Dispatcher { get{ return m_Dispatcher;}}

      /// <summary>
      /// Returns the handler that this filter works under. May be null if the filter works under dispatcher
      /// </summary>
      public WorkHandler Handler { get{ return m_Handler;}}


      /// <summary>
      /// Returns network gate that filter implementation may use to set business variables or null
      /// </summary>
      public INetGate NetGate { get{ return m_Dispatcher.ComponentDirector.Gate;} }


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
      public void FilterWork(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex)
      {
         try
         {
            DoFilterWork(work, filters, thisFilterIndex);
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
      protected abstract void DoFilterWork(WorkContext work, IList<WorkFilter> filters, int thisFilterIndex);


  }
}
