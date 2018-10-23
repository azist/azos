
using Azos.Conf;
using Azos.Wave.Templatization;

namespace Azos.Wave.Handlers
{
  /// <summary>
  /// Implements handler that serves WaveTemplates
  /// </summary>
  public class TemplateHandler : TypeLookupHandler<WaveTemplate>
  {
     #region .ctor

      protected TemplateHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match)
                          : base(dispatcher, name, order, match)
      {
      }

      protected TemplateHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode) : base(dispatcher, confNode)
      {
      }

     #endregion

     #region Protected

      protected override void DoTargetWork(WaveTemplate target, WorkContext work)
      {
        target.BindGlobalContexts(work);
        target.Render(new ResponseRenderingTarget(work), null);
      }

     #endregion

  }
}
