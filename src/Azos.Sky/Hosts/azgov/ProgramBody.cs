using System;
using System.Threading;

using Azos.IO;
using Azos.Log;
using Azos.Platform;

using Azos.Sky.Apps;
using Azos.Sky.Apps.ZoneGovernor;

namespace Azos.Sky.Hosts.azgov
{
  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      try
      {
        SkySystem.MetabaseApplicationName = SysConsts.APP_NAME_ZGOV;

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
      const string FROM = "AZGOV.Program";

      using (var app = new SkyApplication(SystemApplicationType.ZoneGovernor, args, null))
      {
        try
        {
          using (var governor = new ZoneGovernorService())
          {
            governor.Configure(null);
            governor.Start();
            try
            {
              // WARNING: Do not modify what this program reads/writes from/to standard IO streams because
              // AHGOV uses those particular string messages for its protocol
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
                      Topic = SysConsts.LOG_TOPIC_APP_MANAGEMENT,
                      From = FROM,
                      Text = "Main loop received CR|LF. Exiting..."
                    });
                    break;  //<-- AHGOV protocol, AHGOV sends a <CRLF> when it is time to shut down
                  }
                  Thread.Sleep(1000);
                }
              }
              finally
              {
                abortableConsole.Abort();
              }
            }
            finally
            {
              governor.WaitForCompleteStop();
            }
          }//using governor
        }
        catch (Exception error)
        {
          app.Log.Write(new Message
          {
            Type = MessageType.CatastrophicError,
            Topic = SysConsts.LOG_TOPIC_ZONE_MANAGEMENT,
            From = FROM,
            Text = "Exception leaked in run(): " + error.ToMessageWithType(),
            Exception = error
          });

          throw error;
        }
      }//using APP
    }
  }
}
