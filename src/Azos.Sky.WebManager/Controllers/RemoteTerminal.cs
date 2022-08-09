/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos;
using Azos.Wave;
using Azos.Wave.Mvc;

using Azos.Apps.Terminal;
using Azos.Security.Admin;

namespace Azos.Sky.WebManager.Controllers
{
  /// <summary>
  /// Provides AppRemoteTerminal JSON API
  /// </summary>
  public sealed class RemoteTerminal : WebManagerController
  {
      public const string TERMINAL_SESSION_KEY = "app remote terminal instance";


      [Action]
      [RemoteTerminalOperatorPermission]
      public object Connect(string who = null)
      {
        WorkContext.NeedsSession();
        var terminal = WorkContext.Session[TERMINAL_SESSION_KEY] as AppRemoteTerminal;
        if (terminal!=null)
         return new {Status = "Already connected", WhenConnected = terminal.WhenConnected};


        if (who.IsNullOrWhiteSpace()) who = "{0}-{1}".Args(WorkContext.HttpContext.Connection.RemoteIpAddress, WorkContext.Session.User);
        terminal = AppRemoteTerminal.MakeNewTerminal(App);
        var info = terminal.Connect(who);
        WorkContext.Session[TERMINAL_SESSION_KEY] = terminal;
        return info;
      }

      [Action]
      public object Disconnect()
      {
        WorkContext.NeedsSession();
        var terminal = WorkContext.Session[TERMINAL_SESSION_KEY] as AppRemoteTerminal;
        if (terminal==null)
         return new {Status = "Already disconnected"};

        var msg = terminal.Disconnect();
        terminal.Dispose();
        WorkContext.Session[TERMINAL_SESSION_KEY] = null;

        return new {Status = msg};
      }

      [Action]
      [RemoteTerminalOperatorPermission]
      public object Execute(string command)
      {
        WorkContext.NeedsSession();
        var terminal = WorkContext.Session[TERMINAL_SESSION_KEY] as AppRemoteTerminal;
        if (terminal==null)
         return new {Status = "Error", Msg = "Not connected"};

        try
        {
          var result = terminal.Execute(command);

          var plainText = true;
          if (result.IsNotNullOrWhiteSpace())
            if (result.StartsWith(Azos.Apps.Terminal.AppRemoteTerminal.MARKUP_PRAGMA))
            {
              result = result.Remove(0, Azos.Apps.Terminal.AppRemoteTerminal.MARKUP_PRAGMA.Length);
              result = Azos.IO.Console.ConsoleUtils.WriteMarkupContentAsHTML(result);
              plainText = false;
            }

          return new {Status = "OK", PlainText = plainText, Result = result};
        }
        catch(Exception error)
        {
          return new {Status = "Error", Msg = error.Message, Exception = error.ToMessageWithType()};
        }
      }

  }
}
