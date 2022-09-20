/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Wave.Mvc;
using Azos.Security.Admin;
using Azos.Data;
using Azos.Wave;
using Azos.Platform;
using Azos.Web;
using System.Threading.Tasks;

namespace Azos.Apps.Terminal.Web
{
  [NoCache]
  [RemoteTerminalOperatorPermission]
  public class WebConsole : ApiProtocolController
  {
    [Action]
    public async Task Index()
    {
      string esc(string s)
       => s.IsNullOrWhiteSpace() ? "" : s.Replace("\"", "'")
                                         .Replace("<", "&lt;")
                                         .Replace(">","&gt;");

      WorkContext.NeedsSession();
      var html = typeof(Azos.Apps.Terminal.Web.Html).GetText("Console.htm");

      html = html.Replace("[:USER:]", esc(WorkContext.Session.User.Name))
                 .Replace("[:APP:]", esc(App.AppId.Value))
                 .Replace("[:HOST:]", esc(Computer.HostName))
                 .Replace("[:ENV:]", esc(App.EnvironmentName));

      WorkContext.Response.ContentType = ContentType.HTML;
      await WorkContext.Response.WriteAsync(html).ConfigureAwait(false);
    }

    [ActionOnPost(Name = "connection")]
    public object Connection_Connect()
    {
      WorkContext.NeedsSession();
      var who = "{0}-{1}".Args(WorkContext.EffectiveCallerIPEndPoint.Address, WorkContext.Session.User);

      var terminal = AppRemoteTerminal.MakeNewTerminal(App);

      var info = terminal.Connect(who);

      var handle = Guid.NewGuid();
      App.ObjectStore.CheckIn(handle, terminal);

      return GetLogicResult(new {handle, info});
    }

    [ActionOnDelete(Name = "connection")]
    public object Connection_Disconnect(string handle)
    {
      var (terminal, guid) = get(handle);
      terminal.Dispose();
      App.ObjectStore.Delete(guid);

      return GetLogicResult(new { handle });
    }


    [ActionOnPost(Name = "command")]
    public object Command(string handle, string command)
    {
      var (terminal, guid) = get(handle);
      try
      {
        try
        {
          var result = terminal.Execute(command);

          var plainText = true;
          if (result.IsNotNullOrWhiteSpace())
            if (result.StartsWith(AppRemoteTerminal.MARKUP_PRAGMA))
            {
              result = result.Remove(0, AppRemoteTerminal.MARKUP_PRAGMA.Length);
              result = IO.Console.ConsoleUtils.WriteMarkupContentAsHTML(result);
              plainText = false;
            }

          return GetLogicResult(new { success = true, plainText, result });
        }
        catch (Exception error)
        {
          return GetLogicResult(new { success = false, msg = error.Message, exception = new WrappedExceptionData(error) });
        }
      }
      finally
      {
        App.ObjectStore.CheckIn(guid);
      }
    }


    private (AppRemoteTerminal, Guid) get(string handle)
    {
       var guid = handle.AsGUID(Guid.Empty);
       if (guid==Guid.Empty)
       {
         throw HTTPStatusException.NotFound_404("Bad handle");
       }

       var result = App.ObjectStore.CheckOut(guid) as AppRemoteTerminal;
       if (result==null) throw HTTPStatusException.NotFound_404("Bad terminal");

       return (result, guid);
    }

  }
}
