/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Conf;
using Azos.Serialization.JSON;


namespace Azos.Platform.ProcessActivation
{
  public static class SkyProgramBody
  {
    /// <summary>
    ///  Entry point into SKY process activation host program
    /// </summary>
    /// <param name="args">Command line args</param>
    /// <param name="assemblyResolverFixup">Used to install assembly resolution hook to load assemblies from various locations</param>
    public static void Main(string[] args, Action<IConfigSectionNode> assemblyResolverFixup)
    {
      ProgramBodyActivator activator = null;
      try
      {
        Azos.Security.TheSafe.Init(onlyWhenHasNotInitBefore: false, keep: false);//Init default safe config
        activator = new ProgramBodyActivator(args, assemblyResolverFixup.NonNull(nameof(assemblyResolverFixup)));
        activator.Run();
      }
      catch (ProgramBodyActivator.EMissingArgs notfound)
      {
        Console.WriteLine();
        Console.WriteLine(notfound.ToMessageWithType());
        Console.WriteLine();
        generalSyntax();
      }
      catch (ProgramBodyActivator.ENotFound notfound) when (activator != null)
      {
        Console.WriteLine();
        Console.WriteLine(notfound.ToMessageWithType());
        Console.WriteLine();
        generalSyntax();
        dumpPrograms(activator);
      }
      catch (Exception error)
      {
        Console.WriteLine();
        Console.WriteLine(error.ToMessageWithType());
        Console.WriteLine();
        var doc = new WrappedExceptionData(error);
        Console.WriteLine(doc.ToJson(JsonWritingOptions.PrettyPrintRowsAsMapASCII));
        Environment.ExitCode = -1;
      }
    }

    private static void generalSyntax()
    {
      Console.WriteLine("SKY Process Activator v2022.07");
      Console.WriteLine(" on {0} running {1}".Args(Computer.HostName, Abstraction.PlatformAbstractionLayer.PlatformName));
      Console.WriteLine();
      Console.WriteLine("Command syntax:");
      Console.WriteLine();
      Console.WriteLine("  > sky [@cfg] $app_process_name [app-args]");
      Console.WriteLine();
      Console.WriteLine("  @cfg - optional path to config file, e.g. `/opt/sky/toolset1.laconf`");
      Console.WriteLine("         if not specified system looks for co-located `sky.laconf`");
      Console.WriteLine();
      Console.WriteLine("  $app_process_name - is the name of target process, e.g. `gluec`");
      Console.WriteLine();
    }

    private static void dumpPrograms(ProgramBodyActivator activator)
    {
      Console.WriteLine("Available programs:");
      Console.WriteLine("-------------------");
      foreach (var p in activator.All.OrderBy(p => p.bodyAttr.Names.First()))
      {
        Console.WriteLine("${0, -10}  -  {1}".Args(p.bodyAttr.Names.Aggregate("", (e, s) => e + " " + s), p.bodyAttr.Description));
      }
    }
  }
}
