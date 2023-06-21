/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azos.Apps;

using Azos.Collections;
using Azos.Conf;
using Azos.Log;


namespace Azos.Sky.Chronicle.Feed
{
  /// <summary>
  /// Where pulled data is fed into
  /// </summary>
  public abstract class Sink : ApplicationComponent<PullAgentDaemon>, INamed
  {
    public const string CONFIG_SINK_SECTION = "sink";
    public const int MAX_NAME_LEN = 24;

    public Sink(PullAgentDaemon director, IConfigSectionNode cfg) : base(director)
    {
      ConfigAttribute.Apply(this, cfg);
      m_Name.NonBlankMax(MAX_NAME_LEN, nameof(Name));
    }

    protected override void Destructor()
    {
      base.Destructor();
    }

    [Config] private string m_Name;

    public override string ComponentLogTopic => CoreConsts.LOG_TOPIC;

    public string   Name => m_Name;


    /// <summary>
    /// Writes log messages into sink
    /// </summary>
    public abstract Task WriteAsync(IEnumerable<Message> data);
  }
}
