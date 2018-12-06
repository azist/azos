using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;

namespace Azos.Sky.Log.Server
{
  /// <summary>
  /// Maps archive dimensions to/from model of the particular business system
  /// </summary>
  public class LogArchiveDimensionsMapper : ApplicationComponent
  {
    public LogArchiveDimensionsMapper(LogReceiverService director, IConfigSectionNode node) : base(director)
    {
      ConfigAttribute.Apply(this, node);
    }

    public override string ComponentLogTopic => CoreConsts.LOG_TOPIC;

    public virtual Dictionary<string, string> StoreMap(string archiveDimensions) { return null; }
    public virtual Dictionary<string, string> FilterMap(string archiveDimensions) { return null; }
  }
}
