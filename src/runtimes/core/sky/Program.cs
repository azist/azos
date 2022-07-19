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
      new Azos.Platform.Abstraction.NetCore.NetCore20Runtime();
      try
      {
        var activator = new ProgramBodyActivator(args);
        Console.WriteLine("Available programs:");
        Console.WriteLine("-------------------");
        foreach (var p in activator.All.OrderBy(p => p.bodyAttr.Names.First()))
        {
          Console.WriteLine("${0, -10}  -  {1}".Args(p.bodyAttr.Names.Aggregate("", (e, s) => e + " " + s), p.bodyAttr.Description));
        }
        activator.Run();
      }
      catch (ProgramBodyActivator.ENotFound notfound)
      {
        Console.WriteLine(notfound.ToMessageWithType());
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
  }
}

