using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Azos;

namespace Agnivo
{
  public partial class DecodedBSONForm : AgnivoFormBase
  {
    public DecodedBSONForm()
    {
      InitializeComponent();
      InitializeTextBoxContextMenu();
      InitializeFromConfig();
    }

    public DecodedBSONForm(string text) : this()
    {
      DecodedBSONText.Text = text;
    }

    private void InitializeTextBoxContextMenu()
    {
      ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
      var menuItem = new MenuItem("Copy");
      menuItem.Click += new EventHandler(CopyAction);
      contextMenu.MenuItems.Add(menuItem);

      DecodedBSONText.ContextMenu = contextMenu;
    }

    private void CopyAction(object sender, EventArgs e)
    {
      var selected = DecodedBSONText.SelectedText;
      if (selected.IsNullOrEmpty())
        Clipboard.SetText(DecodedBSONText.Text);
      else
        Clipboard.SetText(selected);
    }
  }
}
