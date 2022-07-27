/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Azos;
using Azos.Data;
using Azos.Conf;

namespace Agnivo
{
  public class AgnivoFormBase : System.Windows.Forms.Form, IConfigurable
  {
    private const string CONFIG_WIDTH_ATTR = "width";
    private const string CONFIG_HEIGHT_ATTR = "height";
    private const string CONFIG_LEFT_ATTR = "left";
    private const string CONFIG_TOP_ATTR = "top";
    private const string CONFIG_STATE_ATTR = "state";

    public AgnivoFormBase()
    {
    }

    public virtual void SaveConfiguration(ConfigSectionNode node)
    {
      if (node == null)
        throw new AzosException("{0}.SaveConfiguration(node==null)".Args(this.Name));

      node.AttrByName(CONFIG_WIDTH_ATTR, true).Value = this.Width.AsString();
      node.AttrByName(CONFIG_HEIGHT_ATTR, true).Value = this.Height.AsString();
      node.AttrByName(CONFIG_LEFT_ATTR, true).Value = this.Left.AsString();
      node.AttrByName(CONFIG_TOP_ATTR, true).Value = this.Top.AsString();
      node.AttrByName(CONFIG_STATE_ATTR, true).Value = this.WindowState.AsString();
    }

    public void InitializeFromConfig()
    {
      var settings = SettingsManager.Instance.GetFormSettings(Name);
      Configure(settings);
    }

    public virtual void Configure(IConfigSectionNode node)
    {
      if (node == null || !node.Exists) return;

      var attr = node.AttrByName(CONFIG_WIDTH_ATTR);
      if (attr.Exists)
        this.Width = attr.ValueAsInt(this.Width);

      attr = node.AttrByName(CONFIG_HEIGHT_ATTR);
      if (attr.Exists)
        this.Height = attr.ValueAsInt(this.Height);

      attr = node.AttrByName(CONFIG_LEFT_ATTR);
      if (attr.Exists)
        this.Left = attr.ValueAsInt(this.Left);

      attr = node.AttrByName(CONFIG_TOP_ATTR);
      if (attr.Exists)
        this.Top = attr.ValueAsInt(this.Top);

      attr = node.AttrByName(CONFIG_STATE_ATTR);
      if (attr.Exists)
        this.WindowState = attr.ValueAsEnum<FormWindowState>(FormWindowState.Normal);
    }

    protected override void OnClosed(EventArgs e)
    {
      SettingsManager.Instance.SaveToConfig(this);

      base.OnClosed(e);
    }

    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AgnivoFormBase));
      this.SuspendLayout();
      //
      // AgnivoFormBase
      //
      this.ClientSize = new System.Drawing.Size(284, 261);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "AgnivoFormBase";
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.ResumeLayout(false);

    }
  }
}
