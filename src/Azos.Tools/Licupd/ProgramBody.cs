/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

using System.Text.RegularExpressions;

using Azos.Conf;

namespace Azos.Tools.Licupd
{
  /// <summary>
  /// Lic block updater
  /// </summary>
  [Platform.ProcessActivation.ProgramBody("licupd", Description = "License text updater")]
  public static class ProgramBody
  {
    public static void Main(string[] str_args)
    {
      Console.WriteLine("Azos License Block Updater");
      Console.WriteLine(" Usage:");
      Console.WriteLine("   licupd  path lfile [-pat pattern] [-insert]");
      Console.WriteLine("    path - path to code base");
      Console.WriteLine("    lfile - path to license file");
      Console.WriteLine("    [-pat pattern] - optional pattern search such as '-pat *.txt'. Assumes '*.cs' if omitted");
      Console.WriteLine("    [-insert] - optional switch that causes license block insertion when missing");

      var args = new CommandArgsConfiguration(str_args);

      var path =  args.Root.AttrByIndex(0).ValueAsString(string.Empty);

      var license = File.ReadAllText(args.Root.AttrByIndex(1).ValueAsString(string.Empty));

      var fpat = args.Root["pat"].AttrByIndex(0).ValueAsString("*.cs");

      var insert = args.Root["insert"].Exists;


      var regexp = new Regex(
      @"/\*<FILE_LICENSE>[\s*|\S*]{0,}</FILE_LICENSE>\*/"

      );

      var replaced = 0;
      var inserted = 0;

      foreach(var fn in path.AllFileNamesThatMatch(fpat, true))
      {
       var content = File.ReadAllText(fn);

       if (regexp.Match(content).Success)
       {
          Console.Write("Matched:  " + fn);
          File.WriteAllText(fn, regexp.Replace(content, license));
          Console.WriteLine("   Replaced.");
          replaced++;
       }
       else
        if (insert)
        {
          content = license + System.Environment.NewLine + content;
          File.WriteAllText(fn, content);
          Console.WriteLine("Inserted:  " + fn);
          inserted++;
        }
      }//foreach file

     Console.WriteLine(string.Format("Total Replaced: {0}  Inserted: {1}", replaced, inserted));
     Console.WriteLine("Strike <enter>");
     Console.ReadLine();
    }
  }
}
