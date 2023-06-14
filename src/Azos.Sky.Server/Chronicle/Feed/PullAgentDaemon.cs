/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
  public sealed class PullAgentDaemon : Daemon
  {
    private const string CONFIG_SERVICE_SECTION = "uplink-service";

    private const int RUN_GRANULARITY_MS = 500;
    private const int CHECKPOINT_WRITE_INTERVAL_MS = 10_000;

    public PullAgentDaemon(IApplication application) : base(application) { }
    public PullAgentDaemon(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      cleanupSourcesAndSinks();
      base.Destructor();
    }

    private void cleanupSourcesAndSinks()
    {
      DisposeAndNull(ref m_UplinkService);

      var sources = m_Sources.ToArray();
      m_Sources.Clear();
      sources.ForEach(channel => this.DontLeak(() => channel.Dispose()));

      var sinks = m_Sinks.ToArray();
      m_Sinks.Clear();
      sinks.ForEach(sink => this.DontLeak(() => sink.Dispose()));
    }

    private HttpService m_UplinkService;


    private Task m_NextRun;
    [Config] private string m_DataDir;
    private Registry<Source> m_Sources = new Registry<Source>();
    private Registry<Sink> m_Sinks = new Registry<Sink>();

    public override string ComponentLogTopic => CoreConsts.DATA_TOPIC;

    [Config]
    public string DataDir
    {
      get => m_DataDir;
      set
      {
        CheckDaemonInactive();
        m_DataDir = value;
      }
    }


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      cleanupSourcesAndSinks();
      if (node == null) return;

      var nUplink = node[CONFIG_SERVICE_SECTION];
      m_UplinkService = FactoryUtils.MakeDirectedComponent<HttpService>(this,
                                                                 nUplink,
                                                                 typeof(HttpService),
                                                                 new object[] { nUplink });


      foreach (var nSource in node.ChildrenNamed(Source.CONFIG_SOURCE_SECTION))
      {
        var source = FactoryUtils.MakeDirectedComponent<Source>(this, nSource, typeof(Source), new[]{ nSource });
        m_Sources.Register(source).IsTrue("Unique source `{0}`".Args(source.Name));
      }

      foreach (var nSink in node.ChildrenNamed(Sink.CONFIG_SINK_SECTION))
      {
        var sink = FactoryUtils.MakeDirectedComponent<Sink>(this, nSink, null, new[] { nSink });
        m_Sinks.Register(sink).IsTrue("Unique sink `{0}`".Args(sink.Name)); ;
      }

    }

    protected override void DoStart()
    {
      Name.NonBlank("Configured daemon name");

      m_UplinkService.NonNull("Configured {0}".Args(CONFIG_SERVICE_SECTION));

      (m_Sources.Count > 0).IsTrue("Configured sources");
      (m_Sinks.Count > 0).IsTrue("Configured sinks");
      Directory.Exists(m_DataDir.NonBlank(nameof(DataDir))).IsTrue("Existing data dir");
      m_Sources.All(one => m_Sinks[one.SinkName] != null).IsTrue("All sources pointing to existing sinks");
      m_Sources.All(one => m_UplinkService.Endpoints.Any(ep => ep.RemoteAddress.EqualsOrdIgnoreCase(one.UplinkAddress))).IsTrue("All sources pointing to registered uplink addresses");

      readCheckpoints();

      base.DoStart();
      scheduleNextRun();
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
      var next = Interlocked.Exchange(ref m_NextRun, null);
      if (next != null) next.Await();
      writeCheckpoints(App.TimeSource.UTCNow);
    }

    private void scheduleNextRun()
    {
      if (!Running) return;
      m_NextRun = Task.Delay(RUN_GRANULARITY_MS.ChangeByRndPct(0.25f))
                      .ContinueWith(oneRun);
    }

    private async Task oneRun(Task antecedent)
    {
      if (!Running) return;
      try
      {
        var allSourceTasks = m_Sources.Select(src => processOneSource(src)).ToArray();
        await Task.WhenAll(allSourceTasks).ConfigureAwait(false);

        var utcNow = App.TimeSource.UTCNow;
        if ((utcNow - m_LastCheckpointWriteUtc).TotalMilliseconds > CHECKPOINT_WRITE_INTERVAL_MS)
        {
          writeCheckpoints(utcNow);
        }
      }
      catch(Exception error)
      {
        WriteLog(MessageType.CatastrophicError, nameof(oneRun), "Leaked: " + error.ToMessageWithType(), error);
      }
      finally
      {
        scheduleNextRun();
      }
    }

    private async Task processOneSource(Source source)
    {
      try
      {
        var sink = m_Sinks[source.SinkName];
        if (sink == null) return;//safeguard

        var batch = await source.PullAsync(m_UplinkService).ConfigureAwait(false);
        if (batch.Length == 0) return;

        await sink.WriteAsync(batch).ConfigureAwait(false);
        source.SetCheckpointUtc(batch.Max(one => one.UTCTimeStamp));
      }
      catch(Exception error)
      {
        WriteLog(MessageType.Error, nameof(processOneSource), "Leaked: " + error.ToMessageWithType(), error);
      }
    }

    private void writeCheckpoints(DateTime utcNow)
    {
      this.DontLeak(() => writeCheckpointsUnsafe(utcNow),
                      "Error writing checkpoints to disk: ",
                      nameof(writeCheckpoints),
                      errorLogType: MessageType.CatastrophicError);
    }

    private DateTime m_LastCheckpointWriteUtc;

    private string getFullCheckpointFilePath()
    {
      var fn = $"{nameof(PullAgentDaemon)}.{App.AppId}.{Name}.chkpt";
      var fullFn = Path.Combine(m_DataDir, fn);
      return fullFn;
    }

    private void writeCheckpointsUnsafe(DateTime utcNow)
    {
      var chkChanged = m_Sources.Any(one => one.CheckpointChanged);
      if (!chkChanged) return;

      var fn = getFullCheckpointFilePath();

      var cfg = new LaconicConfiguration();
      cfg.Create("checkpoints");
      cfg.Root.AddAttributeNode("utc-now", utcNow);
      cfg.Root.AddAttributeNode("app-id", App.AppId);
      cfg.Root.AddAttributeNode("agent-id", Name);
      cfg.Root.AddAttributeNode("host", Azos.Platform.Computer.HostName);

      foreach(var source in m_Sources)
      {
        var nSource = cfg.Root.AddChildNode(Source.CONFIG_SOURCE_SECTION);
        nSource.AddAttributeNode("name", source.Name);
        var chkUtc = source.CheckpointUtc;
        nSource.AddAttributeNode("utc-checkpoint", chkUtc.ToString("o"));//ISO8601 with time zone (utc Z)
        nSource.AddAttributeNode("utc-checkpoint-nix-ms", chkUtc.ToMillisecondsSinceUnixEpochStart());
      }
      //SAVE whole log ============================================================
      cfg.SaveAs(fn, CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint);
      //===========================================================================

      //must be last line if there is no exception
      m_Sources.ForEach(one => one.ResetCheckpointChanged());
      m_LastCheckpointWriteUtc = utcNow;
    }

    private void readCheckpoints()
    {
      var fn = getFullCheckpointFilePath();
      if (!File.Exists(fn)) return;

      var data = LaconicConfiguration.ProviderLoadFromFile(fn);

      foreach(var nsrc in data.Root.ChildrenNamed(Source.CONFIG_SOURCE_SECTION))
      {
        var sname = nsrc.ValOf("name");
        var source = m_Sources[sname];
        if (source==null)
        {
          WriteLog(MessageType.Warning, nameof(readCheckpoints), "Checkpoint references source `{0}` which is not in the list of registered pull sources".Args(sname));
          continue;
        }
        source.SetCheckpointUtc(nsrc.Of("utc-checkpoint-nix-ms").ValueAsLong().FromMillisecondsSinceUnixEpochStart());
      }
    }
  }
}
