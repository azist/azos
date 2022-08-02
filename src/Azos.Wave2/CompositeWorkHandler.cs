/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;

using Azos.Conf;
using Azos.Collections;
using System.Threading.Tasks;

namespace Azos.Wave
{
  /// <summary>
  /// Dispatched work to sub-handlers
  /// </summary>
  public sealed class CompositeWorkHandler : WorkHandler
  {
    #region .ctor
    internal CompositeWorkHandler(WaveServer server, IConfigSectionNode confNode) : base(server, confNode){ }

    public CompositeWorkHandler(WorkHandler director, string name, int order, WorkMatch match = null) : base(director, name, order, match)
    {
    }

    public CompositeWorkHandler(WorkHandler director, IConfigSectionNode confNode) : base(director, confNode)
    {
      foreach (var hNode in confNode.Children.Where(cn => cn.IsSameName(WorkHandler.CONFIG_HANDLER_SECTION)))
      {
        var sub = FactoryUtils.MakeDirectedComponent<WorkHandler>(this, hNode, extraArgs: new object[] { hNode });
        if (!m_Handlers.Register(sub))
        {
          throw new WaveException(StringConsts.CONFIG_DUPLICATE_HANDLER_NAME_ERROR.Args(hNode.AttrByName(Configuration.CONFIG_NAME_ATTR).Value));
        }
      }
    }
    protected override void Destructor()
    {
      base.Destructor();
      foreach (var handler in Handlers) handler.Dispose();
    }

    #endregion


    #region Fields
    private OrderedRegistry<WorkHandler> m_Handlers = new OrderedRegistry<WorkHandler>();
    #endregion

    #region Properties
    /// <summary>
    /// Returns ordered registry of handlers
    /// </summary>
    public IOrderedRegistry<WorkHandler> Handlers => m_Handlers;
    #endregion

    #region Public
    /// <summary>
    /// Registers handler and returns true if the named instance has not been registered yet
    /// Note: it is possible to call this method on active server that is - inject handlers while serving requests
    /// </summary>
    public bool RegisterHandler(WorkHandler handler)
    {
      if (handler==null) return false;
      if (handler.ParentHandler != this)
        throw new WaveException(StringConsts.WRONG_HANDLER_HANDLER_REGISTRATION_ERROR.Args(handler));

      return m_Handlers.Register(handler);
    }

    /// <summary>
    /// Unregisters handler and returns true if the named instance has been removed
    /// Note: it is possible to call this method on active server that is - remove handlers while serving requests
    /// </summary>
    public bool UnRegisterHandler(WorkHandler handler)
    {
      if (handler==null) return false;
      if (handler.ParentHandler != this)
        throw new WaveException(StringConsts.WRONG_HANDLER_HANDLER_UNREGISTRATION_ERROR.Args(handler));

      return m_Handlers.Unregister(handler);
    }

    /// <summary>
    /// Unregisters handler and returns true if the named instance has been removed
    /// Note: it is possible to call this method on active server that is - remove handlers while serving requests
    /// </summary>
    public bool UnRegisterHandler(string name)
    {
      if (name.IsNullOrWhiteSpace()) return false;
      return m_Handlers.Unregister(name);
    }

    #endregion

    #region Protected

    protected override async Task DoHandleWorkAsync(WorkContext work)
    {
      var subHandler = m_Handlers.OrderedValues.FirstOrDefault(handler => handler.MakeMatch(work));

      if (subHandler == null)
        throw HTTPStatusException.NotFound_404(StringConsts.NO_HANDLER_ERROR);

      await subHandler.FilterAndHandleWorkAsync(work).ConfigureAwait(false);
    }
    #endregion
  }
}
