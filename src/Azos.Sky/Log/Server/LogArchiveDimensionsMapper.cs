/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
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
