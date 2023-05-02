using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Azos.Data;
using Azos.Instrumentation;

namespace WinFormsTest
{
  public partial class EMAForm : System.Windows.Forms.Form
  {
    public EMAForm()
    {
      InitializeComponent();
      m_Monitor = Azos.Apps.ExecutionContext.Application.ModuleRoot.Get<ISystemLoadMonitor<SysLoadSample>>();
    }

    private EmaLong m_Width = new EmaLong(0.1, 0, 0);
    ISystemLoadMonitor<SysLoadSample> m_Monitor;

    private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
    {
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      hScrollBar1.Maximum = this.Width;
      var v = hScrollBar1.Value;
      btn.Left = v;
      EmaLong.AddNext(ref m_Width, v);
      btnEMA.Left = (int)m_Width.Average;

      var s = chkInstant.Checked ? m_Monitor.CurrentSample : m_Monitor.DefaultAverage;

      btnCpu.Left = (int)(Width * s.CpuLoadPercent);
      btnRAM.Left = (int)(Width * s.RamLoadPercent);
      btnCpu.Text = $"{s.CpuLoadPercent*100:n} %";
      btnRAM.Text = $"{s.RamLoadPercent*100:n} %";
    }

    private void tbFactor_TextChanged(object sender, EventArgs e)
    {
      m_Width = new EmaLong(tbFactor.Text.AsDouble(), 0, 0);
    }
  }
}
