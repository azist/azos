/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace ard
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static void Main(string[] args)
    {
      if (Environment.UserInteractive)
      {
        try
        {
          run(args);
          Environment.ExitCode = 0;
        }
        catch (Exception error)
        {
          Console.WriteLine(error.ToString());
          Environment.ExitCode = -1;
        }
      }
      else
      {
        ServiceBase[] ServicesToRun;
        ServicesToRun = new ServiceBase[] { new ARDService() };
        ServiceBase.Run(ServicesToRun);
      }
    }

    static void run(string[] args)
    {
      if (args.Length == 1 && "/i".Equals(args[0], StringComparison.OrdinalIgnoreCase))
      {
        ARDInstaller.Install();
      }
      else if (args.Length == 1 && "/u".Equals(args[0], StringComparison.OrdinalIgnoreCase))
      {
        ARDInstaller.Uninstall();
      }
      else
      {
        ARDService srv = new ARDService();
        srv.TestStartupAndStop(args);
      }
    }
  }
}
