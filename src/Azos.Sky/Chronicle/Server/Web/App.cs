/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;


using Azos.Platform;
using Azos.Security;

using Azos.Sky.Security.Permissions.Chronicle;
using Azos.Wave.Mvc;
using Azos.Web;

namespace Azos.Sky.Chronicle.Server.Web
{
  [NoCache]
  [ChroniclePermission]
  [ApiControllerDoc(
    BaseUri = "/chronicle/app",
    Connection = "default/keep alive",
    Title = "Chronicle App",
    Authentication = "Token/Default",
    Description = "Provides API for Chronicle app",
    TypeSchemas = new[]{typeof(ChroniclePermission) }
  )]
  //[Release(ReleaseType.Preview, 2020, 07, 05, "Initial Release", Description = "Preview release of API")]
  public class App : ApiProtocolController
  {
    [Action(Name = "log")]
    public void Log()
    {
      string esc(string s)
        => s.IsNullOrWhiteSpace() ? "" : s.Replace("\"", "'")
                                          .Replace("<", "&lt;")
                                          .Replace(">", "&gt;");

      WorkContext.NeedsSession();
      var html = typeof(App).GetText("LogView.htm");

      html = html.Replace("[:USER:]", esc(WorkContext.Session.User.Name))
                 .Replace("[:APP:]", esc(App.AppId.Value))
                 .Replace("[:HOST:]", esc(Computer.HostName))
                 .Replace("[:ENV:]", esc(App.EnvironmentName));

      WorkContext.Response.ContentType = ContentType.HTML;
      WorkContext.Response.Write(html);
    }
  }
}
