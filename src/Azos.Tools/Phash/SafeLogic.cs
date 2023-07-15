/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;

using Azos.Apps;
using Azos.IO.Console;
using Azos.Security;
using Azos.Serialization.JSON;
using Azos.Platform;
using Azos.Conf;
using System.Linq;

namespace Azos.Tools.Phash
{
  public static class SafeLogic
  {
    public static string GetPassword(IConfigSectionNode argsSafe, bool doConfirm)
    {
      var result = argsSafe.ValOf("pwd");
      Console.WriteLine();
      while(result.IsNullOrWhiteSpace())
      {
        Console.WriteLine("Type-in secure password: ");
        result = ConsoleUtils.ReadPassword('*');
      }
      Console.WriteLine();

      if (doConfirm)
      {
        ConsoleUtils.Warning("Make sure you remember the password and confirm it!");
        ConsoleUtils.Warning("Otherwise your content will not be deciphered back if you delete the original unprotected files");
        Console.WriteLine();
        ConsoleUtils.Info("You can hit CTRL+C now to abort if needed");
        Console.WriteLine();
        string confirm;
        do
        {
          Console.WriteLine("Re type secure password to confirm: ");
          confirm = ConsoleUtils.ReadPassword('#');
        }
        while(!result.EqualsOrdSenseCase(confirm));
      }

      Console.WriteLine();
      return result;
    }

    public static void Protect(IApplication app, IConfigSectionNode argsSafe)
    {
      var pwd = GetPassword(argsSafe, true);
      TheSafe.ProtectDirectory(argsSafe.AttrByIndex(0).Value,
                               pwd,
                               argsSafe.ValOf("pfx").Default(TheSafe.FILE_PREFIX_SAFE),
                               argsSafe.ValOf("ext").Default(TheSafe.FILE_EXTENSION_SAFE),
                               argsSafe.Of("recurse").ValueAsBool(false),
                               argsSafe.Of("delete").ValueAsBool(false),
                               (fn, cfn, err) =>
                               {
                                 if (err == null)
                                 {
                                   ConsoleUtils.WriteMarkupContent("Protect <f color=darkCyan>`{0}`<f color=gray> -> <f color=cyan>`{1}`<f color=gray>\n".Args(fn, cfn));
                                   return true;
                                 }

                                 ConsoleUtils.WriteMarkupContent("!!! Error protecting <f color=red>`{0}`<f color=gray> \n Error text: <f color=yellow> {1}<f color=gray>\n".Args(fn, err.ToMessageWithType()));
                                 return true;
                               });
    }

    public static void Unprotect(IApplication app, IConfigSectionNode argsSafe)
    {
      var pwd = GetPassword(argsSafe, false);
      TheSafe.UnprotectDirectory(argsSafe.AttrByIndex(0).Value,
                                pwd,
                                argsSafe.ValOf("pfx").Default(TheSafe.FILE_PREFIX_SAFE),
                                argsSafe.ValOf("ext").Default(TheSafe.FILE_EXTENSION_SAFE),
                                argsSafe.Of("recurse").ValueAsBool(false),
                                argsSafe.Of("delete").ValueAsBool(false),
                                (fn, cfn, err) =>
                                {
                                  if (err == null)
                                  {
                                    ConsoleUtils.WriteMarkupContent("Unprotect <f color=cyan>`{0}`<f color=gray> -> <f color=darkCyan>`{1}`<f color=gray>\n".Args(fn, cfn));
                                    return true;
                                  }

                                  ConsoleUtils.WriteMarkupContent("!!! Error unprotecting <f color=red>`{0}`<f color=gray> \n Error text: <f color=yellow> {1}<f color=gray>\n".Args(fn, err.ToMessageWithType()));
                                  return true;
                                });
    }

  }
}
