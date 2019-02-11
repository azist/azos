/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Conf;
using Azos.Collections;

namespace Azos.Wave.Handlers
{
  /// <summary>
  /// Dispatched work to sub-handlers just like dispatcher does
  /// </summary>
  public sealed class CompositeHandler : WorkHandler
  {
    #region .ctor
      public CompositeHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match) : base(dispatcher, name, order, match)
      {

      }

      public CompositeHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode)
      {
      foreach (var hNode in confNode.Children.Where(cn => cn.IsSameName(WorkHandler.CONFIG_HANDLER_SECTION)))
      {
        var sub = FactoryUtils.Make<WorkHandler>(hNode, args: new object[] { dispatcher, hNode });
        sub.___setParentHandler(this);
#warning Refactor: REMOVE per Wave.ASYNC
        sub.__setComponentDirector(this);
        if (!m_Handlers.Register(sub))
          throw new WaveException(StringConsts.CONFIG_DUPLICATE_HANDLER_NAME_ERROR.Args(hNode.AttrByName(Configuration.CONFIG_NAME_ATTR).Value));
      }

    }
    #endregion


    #region Fields

         private OrderedRegistry<WorkHandler> m_Handlers = new OrderedRegistry<WorkHandler>();

    #endregion

    #region Properties

        /// <summary>
        /// Returns ordered registry of handlers
        /// </summary>
        public IRegistry<WorkHandler> Handlers { get { return m_Handlers;}}

    #endregion

    #region Public
       /// <summary>
        /// Registers handler and returns true if the named instance has not been registered yet
        /// Note: it is possible to call this method on active server that is - inject handlers while serving requests
        /// </summary>
        public bool RegisterHandler(WorkHandler handler)
        {
          if (handler==null) return false;
          if (handler.Dispatcher!=this.Dispatcher)
            throw new WaveException(StringConsts.WRONG_DISPATCHER_HANDLER_REGISTRATION_ERROR.Args(handler));

          return m_Handlers.Register(handler);
        }

        /// <summary>
        /// Unregisters handler and returns true if the named instance has been removed
        /// Note: it is possible to call this method on active server that is - remove handlers while serving requests
        /// </summary>
        public bool UnRegisterHandler(WorkHandler handler)
        {
          if (handler==null) return false;
          if (handler.Dispatcher!=this.Dispatcher)
            throw new WaveException(StringConsts.WRONG_DISPATCHER_HANDLER_UNREGISTRATION_ERROR.Args(handler));

          return m_Handlers.Unregister(handler);
        }

    #endregion

    #region Protected

      protected override void DoHandleWork(WorkContext work)
      {
        var subHandler = m_Handlers.OrderedValues.FirstOrDefault(handler => handler.MakeMatch(work));

        if (subHandler==null)
          throw HTTPStatusException.NotFound_404(StringConsts.NO_HANDLER_FOR_WORK_ERROR.Args(work.About));

        subHandler.FilterAndHandleWork(work);
      }

    #endregion

  }

}
