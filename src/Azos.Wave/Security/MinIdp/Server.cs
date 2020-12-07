/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Instrumentation;
using Azos.Wave;
using Azos.Wave.Mvc;

namespace Azos.Security.MinIdp
{
  /// <summary>
  /// Controller for serving IMinIdpStore remotely
  /// </summary>
  [NoCache]
  [IdpPermission]
  [ApiControllerDoc(
    BaseUri = "/minidp/server",
    Connection = "default/keep alive",
    Title = "MinIdp Server",
    Authentication = "Token/Default",
    Description = "Provides API for accessing MinIdp store remotely",
    TypeSchemas = new[] { typeof(IdpPermission) }
  )]
  [Release(ReleaseType.Preview, 2020, 12, 01, "Initial Release", Description = "Preview release of API")]
  public sealed class Server : ApiProtocolController
  {
    private MinIdpSecurityManager Secman
     => (App.SecurityManager as MinIdpSecurityManager).NonNull("App.SecMan is not {0}".Args(nameof(MinIdpSecurityManager)));

    private IMinIdpStore Store
    {
      get
      {
        IMinIdpStoreContainer container = Secman;
        while(true)
        {
          var result = container.Store;

          result.NonNull("Configuration error");

          if (result is IMinIdpStoreContainer next)
          {
            container = next;
            continue;
          }

          return result;
        }
      }
    }

    [Action(Name = "byid")]
    public async Task<object> GetById(Atom realm, string id)
     => this.GetLogicResult(await Store.GetByIdAsync(realm, id));

    [Action(Name = "bysys")]
    public async Task<object> GetBySys(Atom realm, string sysToken)
    => this.GetLogicResult(await Store.GetBySysAsync(realm, sysToken));

    [Action(Name = "byuri")]
    public async Task<object> GetByUri(Atom realm, string uri)
    => this.GetLogicResult(await Store.GetByUriAsync(realm, uri));

    [ActionOnPost(Name = "exec")]
    public void ExecCommand(Atom realm, string cmd)
    {
      IConfigSectionNode command = null;

      try
      {
        command = cmd.NonBlank(nameof(cmd))
                     .AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
      }
      catch(Exception error)
      {
        throw HTTPStatusException.BadRequest_400("Bad command syntax: " + error.ToMessageWithType());
      }

      var callable = (Store as IExternallyCallable).NonNull("Is not {0}".Args(nameof(IExternallyCallable)));
      var handler = callable.GetExternalCallHandler();
      var got = handler.HandleRequest(command);

      WorkContext.Response.StatusCode = got.StatusCode;
      WorkContext.Response.StatusDescription = got.StatusDescription;
      WorkContext.Response.ContentType = got.ContentType;
      WorkContext.Response.Write(got.Content);
    }
  }
}
