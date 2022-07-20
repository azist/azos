/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;

using Azos;
using Azos.Platform.ProcessActivation;
using Azos.Serialization.JSON;

namespace sky
{
  class Program
  {
    static void Main(string[] args)
    {
      new Azos.Platform.Abstraction.NetFramework.DotNetFrameworkRuntime();
      ProgramBodyActivator activator = null;
      try
      {
        activator = new ProgramBodyActivator(args);
        activator.Run();
      }
      catch(ProgramBodyActivator.EMissingArgs notfound)
      {
        Console.WriteLine(notfound.ToMessageWithType());
        generalSyntax();
      }
      catch(ProgramBodyActivator.ENotFound notfound) when (activator != null)
      {
        Console.WriteLine(notfound.ToMessageWithType());
        dumpPrograms(activator);
      }
      catch(Exception error)
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
      Console.WriteLine("Process activator general syntax:");
      Console.WriteLine("  > sky $app_process_name [app-args]");
      Console.WriteLine("");
      Console.WriteLine("  $app_process_name - is the name of target process, e.g. `gluec`");
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
