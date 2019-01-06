/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Windows.Forms;

using Azos;
using Azos.Data;

using Azos.Sky;
using Azos.Sky.Workers;

namespace WinFormsTestSky.Workers
{
  public partial class ProcessForm : System.Windows.Forms.Form
  {
    public ProcessForm()
    {
      InitializeComponent();
    }

    private Azos.Sky.Workers.Server.ProcessControllerService m_Server;

    ISkyApplication App => Azos.WinForms.FormsAmbient.App.AsSky();


    private PID doAllocate(string zonePath, string id, bool isUnique)
    {
      var zone = App.Metabase.CatalogReg.NavigateZone(zonePath);
      var processorID = zone.MapShardingKeyToProcessorID(id);

      return new PID(zone.RegionPath, processorID, id.ToString(), isUnique);
    }

    private void btnSpawn_Click(object sender, EventArgs e)
    {
      var pid = App.ProcessManager.Allocate("us/east/cle/a/ii");
      var process = Process.MakeNew<TeztProcess>(App, pid);
      var host = m_Server as IProcessHost;
      host.LocalSpawn(process);
    }

    private void btnServerStart_Click(object sender, EventArgs e)
    {
      var cfg = @"
srv
{
  startup-delay-sec = 1

  process-store
  {
    type='Azos.Sky.Workers.Server.Queue.MongoProcessStore, Azos.Sky.MongoDB'
    mongo='mongo{server=\'localhost:27017\' db=\'process-tezt\'}'
  }
}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      m_Server = new Azos.Sky.Workers.Server.ProcessControllerService(App);
      m_Server.Configure(cfg);
      m_Server.Start();
    }

    private void btnServerStop_Click(object sender, EventArgs e)
    {
      DisposableObject.DisposeAndNull(ref m_Server);
    }

    private void tmr_Tick(object sender, EventArgs e)
    {
      var enabled = m_Server != null;

      btnServerStart.Enabled = !enabled;
      btnServerStop.Enabled = enabled;
      btnSpawn.Enabled = enabled;

      if (enabled)
        lstProcess.DataSource = m_Server.List(0);
    }
  }
}
