/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Serialization.JSON;


namespace Azos.Platform.ProcessActivation
{
  public static class SkyProgramBody
  {
    public static void Main(string[] args)
    {
      ProgramBodyActivator activator = null;
      try
      {
        activator = new ProgramBodyActivator(args);
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
        dumpPrograms(activator);
      }
      catch (Exception error)
      {
        var doc = new WrappedExceptionData(error);
        Console.WriteLine(error.ToMessageWithType());
        Console.WriteLine();
        Console.WriteLine(doc.ToJson(JsonWritingOptions.PrettyPrintRowsAsMapASCII));
        Environment.ExitCode = -1;
      }
    }

    private static void generalSyntax()
    {
      Console.WriteLine("SKY Process Activator v2022.07");
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
