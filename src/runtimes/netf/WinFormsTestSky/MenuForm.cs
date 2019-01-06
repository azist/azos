/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Windows.Forms;

namespace WinFormsTestSky
{
  public partial class MenuForm : Form
  {
    public MenuForm()
    {
      InitializeComponent();
    }

    private void btnCache_Click(object sender, EventArgs e)
    {
      new Caching.CacheForm().Show();
    }

    private void btnLocking_Click(object sender, EventArgs e)
    {
      new Locker.MDSARLocking().Show();
    }

    private void btnGDID_Click(object sender, EventArgs e)
    {
      new IDGen.GDIDForm().Show();
    }

    private void btnKDB_Click(Object sender,EventArgs e)
    {
      new KDB.KDBForm().Show();
    }

    private void btnTodo_Click(object sender, EventArgs e)
    {
      new Workers.TodoForm().Show();
    }

    private void btnProcess_Click(object sender, EventArgs e)
    {
      new Workers.ProcessForm().Show();
    }

  }
}
