using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace ard
{
  [RunInstaller(true)]
  public class ARDInstaller : Installer
  {
    public ARDInstaller()
    {
      var svcInst = new ServiceInstaller
      {
        Description = ARDService.SERVICE_DESCRIPTION,
        DisplayName = ARDService.DISPLAY_NAME,
        ServiceName = ARDService.SERVICE_NAME,
        StartType = ServiceStartMode.Automatic
      };
      var svcProcInst = new ServiceProcessInstaller { Account = ServiceAccount.LocalSystem };
      var elInst = new EventLogInstaller
      {
        Source = ARDService.SERVICE_NAME,
        Log = "Application"
      };

      Installers.AddRange(new Installer[] { svcInst, svcProcInst, elInst });
    }

    internal static void Install()
    {
      var assembly = typeof(ARDService).Assembly;
      var logFile = assembly.GetName().Name + ".InstallLog";
      var installer = new AssemblyInstaller(assembly, new[] { "/LogToConsole=false", "/LogFile=" + logFile});
      installer.UseNewContext = true;

      var data = new Hashtable();
      try
      {
        installer.Install(data);
        installer.Commit(data);
        Console.WriteLine("Installed.");
      }
      catch (ArgumentException)
      {
        Console.WriteLine(string.Format("See log: {0}", logFile));
      }
    }

    internal static void Uninstall()
    {
      var assembly = typeof(ARDService).Assembly;
      var logFile = assembly.GetName().Name + ".UninstallLog";
      var installer = new AssemblyInstaller(assembly, new[] { "/LogToConsole=false", "/LogFile=" + logFile });
      installer.UseNewContext = true;

      try
      {
        installer.Uninstall(null);
        Console.WriteLine("Uninstalled.");
      }
      catch (ArgumentException)
      {
        Console.WriteLine(string.Format("See log: {0}", logFile));
      }
    }
  }
}
