/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Apps;
using Azos.Conf;
using Azos.IO.Console;
using Azos.Security;
using Azos.Glue;
using Azos.Glue.Protocol;
using Azos.Platform;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Tools.idp
{
  [Platform.ProcessActivation.ProgramBody("idp", Description = "AuthKit IDentity Provider CLI tool")]
  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      try
      {
        using (var app = new AzosApplication(args, null))
        {
          run(app);
        }
      }
      catch (Exception error)
      {
        ConsoleUtils.Error(new WrappedExceptionData(error).ToJson(JsonWritingOptions.PrettyPrintRowsAsMapASCII));

        Environment.ExitCode = -1;
      }
    }

    static void run(IApplication app)
    {
      var silent = app.CommandArgs["s", "silent"].Exists;
      if (!silent)
      {
        ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Welcome.txt"));

        ConsoleUtils.Info("Build: " + BuildInformation.ForFramework);
        ConsoleUtils.Info("App conf descr: " + app.Description);
      }

      if (app.CommandArgs["?", "h", "help"].Exists)
      {
        ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Help.txt"));
        return;
      }

      IIdpUserAdminLogic logic = app.ModuleRoot.TryGet<IIdpUserAdminLogic>();
      if (logic == null)
      {
        throw new AuthKitException("The required `IIdpUserAdminLogic` module could not be resolved. \n" +
         "We need to know what IDP server to connect to, using what corresponding credentials, which are supplied via configured app module instance. \n" +
         "Did you forget to run the tool with the config switch pointing to an appropriate file e.g. `-config ~/your-idp-environment.laconf`? ");
      }

      var verbName = app.CommandArgs.AttrByIndex(0).Value;
      if (verbName.IsNullOrWhiteSpace())
      {
        throw new AuthKitException("Dont know what to do - missing verb. Try adding something like `userlist`");
      }

      var tname = "Azos.AuthKit.Tools.idp.{0}Verb, Azos.AuthKit".Args(verbName);
      var tverb = Type.GetType(tname, throwOnError: false, ignoreCase: true);
      if (tverb == null || !tverb.IsSubclassOf(typeof(Verb)))
      {
        throw new AuthKitException("Dont know how to handle `{0}`".Args(verbName));
      }
      var verb = Activator.CreateInstance(tverb, logic, silent) as Verb;
      try
      {

        verb.Run();
      }
      finally
      {
        DisposableObject.DisposeIfDisposableAndNull(ref verb);
      }
    }//run
  }
}
