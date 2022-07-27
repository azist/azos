/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Windows.Forms;

using Azos;
using Azos.Apps;
using Azos.Serialization.JSON;

namespace Agnivo
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      try
      {
        new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
        using (var application = new AzosApplication(args, null))
        {
          Application.EnableVisualStyles();
          Application.SetCompatibleTextRenderingDefault(false);
          Application.Run(new MainForm());
        }
      }
      catch(Exception error)
      {
        MessageBox.Show(new WrappedExceptionData(error).ToJson(JsonWritingOptions.PrettyPrintRowsAsMap));
      }
    }
  }
}
