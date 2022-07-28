/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
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
    //Net 6 upgrade
      //ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();
      //var menuItem = new MenuItem("Copy");
      //menuItem.Click += new EventHandler(CopyAction);
      //contextMenu.MenuItems.Add(menuItem);

      //DecodedBSONText.ContextMenu = contextMenu;
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
