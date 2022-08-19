/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Threading;

using Azos.IO.Console;
using Azos.Log;
using Azos.Platform;

using Azos.Apps;
using Azos.Sky.Identification;

namespace Azos.Sky.Hosts.agdida
{
  [Platform.ProcessActivation.ProgramBody("agdida", Description = "Gdid authority")]
  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      try
      {
        SkySystem.MetabaseApplicationName = SysConsts.APP_NAME_GDIDA;

        run(args);

        Environment.ExitCode = 0;
      }
      catch (Exception error)
      {
        Console.WriteLine(error.ToString());
        Environment.ExitCode = -1;
      }
    }

    static void run(string[] args)
    {
      const string FROM = "AGDIDA.Program";

      using (var app = new SkyApplication(SystemApplicationType.GDIDAuthority, args, null))
      {
        try
        {
          using (var authority = new Identification.Server.GdidAuthorityService(app))
          {
            authority.Configure(null);
            authority.Start();
            try
            {
              // WARNING: Do not modify what this program reads/writes from/to standard IO streams because
              //  AHGOV uses those particular string messages for its protocol
              Console.WriteLine("OK."); //<-- AHGOV protocol, AHGOV waits for this token to assess startup situation
              ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Welcome.txt"));
              Console.WriteLine("Waiting for line to terminate...");

              var abortableConsole = new TerminalUtils.AbortableLineReader();
              try
              {
                while (app.Active)
                {
                  if (abortableConsole.Line != null)
                  {
                    app.Log.Write(new Message
                    {
                      Type = MessageType.Info,
                      Topic = CoreConsts.TOPIC_ID_GEN,
                      From = FROM,
                      Text = "Main loop received CR|LF. Exiting..."
                    });
                    break;  //<-- AHGOV protocol, AHGOV sends a <CRLF> when it is time to shut down
                  }
                  Thread.Sleep(250);
                }
              }
              finally
              {
                abortableConsole.Abort();
              }
            }
            finally
            {
              authority.WaitForCompleteStop();
            }
          }
        }
        catch (Exception error)
        {
          app.Log.Write(new Message
          {
            Type = MessageType.CatastrophicError,
            Topic = CoreConsts.TOPIC_ID_GEN,
            From = FROM,
            Text = "Exception leaked in run(): " + error.ToMessageWithType(),
            Exception = error
          });

          throw;
        }
      }//using app
    }
  }
}
