/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;
using System.Linq;

using Azos.Apps;
using Azos.IO.Console;
using Azos.Security;
using Azos.Conf;
using Azos.Data;

namespace Azos.Tools.Phash
{
  public static class SafeLogic
  {
    public static string GetPassword(IConfigSectionNode argsSafe, bool doConfirm)
    {
      var result = argsSafe.ValOf("pwd");
      if (result.IsNotNullOrWhiteSpace()) return  result;

      while(result.IsNullOrWhiteSpace())
      {
        Console.WriteLine();
        Console.WriteLine("Please type-in the secure password: ");
        result = ConsoleUtils.ReadPassword('*');
      }
      Console.WriteLine();

      if (doConfirm)
      {
        ConsoleUtils.Info("You will now need to retype your password again a few times");
        ConsoleUtils.Warning("Make sure you remember the password and confirm it properly,");
        ConsoleUtils.Warning("otherwise your content will not be decipherable back if you delete the original unprotected files and forget the password");
        Console.WriteLine();
        ConsoleUtils.Warning("This is irrevocable operation and there is no way to recover your password!");
        Console.WriteLine();
        ConsoleUtils.Info("You can hit CTRL+C now to abort if needed");
        Console.WriteLine();
        string confirm;
        for(int i=0; i < 3; i++)
        {
          Console.WriteLine("Type your secure password again to confirm or hit CTRL+C to abort if unsure:");
          confirm = ConsoleUtils.ReadPassword('#');
          Console.WriteLine();
          if (!result.EqualsOrdSenseCase(confirm))
          {
            ConsoleUtils.Error("Passwords do not match. Hit CTRL+C if you need to abort, or retype your password again");
            i = 0;
          }
        }
      }

      Console.WriteLine();
      return result;
    }

    public static string GetValue(IConfigSectionNode argsSafe)
    {
      var result = argsSafe.Attributes.FirstOrDefault(one => one.Name.StartsWith("?"))?.Value;
      if (result.IsNotNullOrWhiteSpace()) return result;
      Console.WriteLine();
      Console.WriteLine("Please provide a value to process: ");
      result = Console.ReadLine();
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

    public static void Cipher(IApplication app, IConfigSectionNode argsSafe)
    {
      using var s = new Security.SecurityFlowScope(TheSafe.SAFE_ACCESS_FLAG);
      var val = GetValue(argsSafe);
      var alg = argsSafe.ValOf("alg", "algo", "algorithm");
      var text = argsSafe.Of("text").ValueAsBool(true);
      var got = text ? TheSafe.CipherConfigValue(val, alg) : TheSafe.CipherConfigValue(val.AsByteArray(), alg);
      Console.WriteLine("Ciphered:");
      Console.WriteLine(got ?? "<null>");
      Console.WriteLine();
    }

    public static void Decipher(IApplication app, IConfigSectionNode argsSafe)
    {
      using var s = new Security.SecurityFlowScope(TheSafe.SAFE_ACCESS_FLAG);
      var val = GetValue(argsSafe);
      var alg = argsSafe.ValOf("alg", "algo", "algorithm");
      var text = argsSafe.Of("text").ValueAsBool(true);
      object got = TheSafe.DecipherConfigValue(val, text, alg);
      Console.WriteLine("Deciphered:");
      Console.WriteLine(got ?? "<null>");
      Console.WriteLine();
    }
  }
}
