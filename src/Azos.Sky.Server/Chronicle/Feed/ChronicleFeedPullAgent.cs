/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.Client;
using Azos.Collections;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Idgen;
using Azos.Log;
using Azos.Serialization.JSON;

namespace Azos.Sky.Chronicle.Feed
{
  /// <summary>
  /// Pulls chronicle data feed from X number of channels into local receptacle
  /// </summary>
  public sealed class ChronicleFeedPullAgent : ModuleBase
  {
    public ChronicleFeedPullAgent(IApplication application) : base(application) { }
    public ChronicleFeedPullAgent(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      cleanupChannels();
      base.Destructor();
    }

    private void cleanupChannels()
    {
      var channels = m_Channels.ToArray();
      m_Channels.Clear();
      channels.ForEach(channel => this.DontLeak(() => channel.Dispose()));
    }

    [Inject] ILogChronicleLogic m_Chronicle;


    [Config] private Atom m_UpstreamId;
    [Config] private string m_DataDir;
    private AtomRegistry<Channel> m_Channels = new AtomRegistry<Channel>();

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;

    public Atom UpstreamId => m_UpstreamId;
    public string DataDir => m_DataDir;


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      cleanupChannels();
      if (node == null) return;

      foreach(var nChannel in node.ChildrenNamed(Channel.CONFIG_CHANNEL_SECTION))
      {
        var channel = FactoryUtils.MakeDirectedComponent<Channel>(this, nChannel, typeof(Channel), new[]{ nChannel });
        m_Channels.Register(channel);
      }

    }

    protected override bool DoApplicationAfterInit()
    {
      m_UpstreamId.HasRequiredValue("Configured upstream id");
      (m_Channels.Count > 0).IsTrue("Configured channels");
      Directory.Exists(m_DataDir.NonBlank(nameof(DataDir))).IsTrue("Existing data dir");
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      return base.DoApplicationBeforeCleanup();
    }

    private void writeCheckpoints()
    {
      this.DontLeak(() => writeCheckpointsUnsafe(),
                      "Error writing checkpoints to disk: ",
                      nameof(writeCheckpoints),
                      errorLogType: MessageType.CatastrophicError);
    }

    private void writeCheckpointsUnsafe()
    {
      var fn = $"{nameof(ChronicleFeedPullAgent)}.{App.AppId}.{m_UpstreamId}.chkpt";
      var fullFn = Path.Combine(m_DataDir, fn);

      var cfg = new LaconicConfiguration();
      cfg.Create("checkpoints");
      cfg.Root.AddAttributeNode("utc-now", App.TimeSource.UTCNow);
      cfg.Root.AddAttributeNode("host", Azos.Platform.Computer.HostName);

      foreach(var channel in m_Channels)
      {
        var nChannel = cfg.Root.AddChildNode(Channel.CONFIG_CHANNEL_SECTION);
        nChannel.AddAttributeNode("name", channel.Name);
        nChannel.AddAttributeNode("utc-checkpoint", channel.CheckpointUtc);
      }

      cfg.SaveAs(fullFn, CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint);
    }


  }
}
