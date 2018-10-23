
using Azos.Conf;

namespace Azos.Wave.Handlers
{
  /// <summary>
  /// Implements handler that does nothing
  /// </summary>
  public sealed class NOPHandler : WorkHandler
  {
     #region .ctor

      public NOPHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match)
                          : base(dispatcher, name, order, match)
      {
      }

      public NOPHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode)
      {
      }

     #endregion

     #region Protected

      protected override void DoHandleWork(WorkContext work)
      {

      }

     #endregion

  }
}
