/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Windows.Forms;

using Azos;
using Azos.Apps;
using Azos.Sky;

namespace WinFormsTestSky
{
  static class Program
  {
    [STAThread]
    static void Main()
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();

      SkySystem.MetabaseApplicationName = "WinFormsTestSky";

      try
      {
        run();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "err", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private static void run()
    {
      using (var app = new SkyApplication(SystemApplicationType.TestRig, new string[] {}, null))
      {
        ((Azos.Sky.Identification.GdidGenerator)app.GdidProvider).TestingAuthorityNode = "sync://127.0.0.1:4000";
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MenuForm());
      }
    }
  }
}
