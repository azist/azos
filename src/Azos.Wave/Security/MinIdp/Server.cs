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
  [Release(ReleaseType.Release, 2020, 12, 24, "Release", Description = "RTM release")]
  public sealed class Server : ApiProtocolController
  {
    private MinIdpSecurityManager Secman
     => (App.SecurityManager as MinIdpSecurityManager).NonNull("App.SecMan is not {0}".Args(nameof(MinIdpSecurityManager)));

    private IMinIdpStore Store => Secman.Store;

    private IMinIdpStore TargetStore
    {
      get
      {
        IMinIdpStoreContainer container = Secman;
        while (true)
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


    private object secure(IMinIdpStore store, object data, bool plain)
    {
      var crypt = store.MessageProtectionAlgorithm;
      if (crypt != null && !(Ambient.CurrentCallUser.Status >= UserStatus.System && plain))
        data = crypt.ProtectAsString(data, Serialization.JSON.JsonWritingOptions.CompactRowsAsMap);

      return this.GetLogicResult(data);
    }

    [ApiEndpointDoc(
      Title = "MinIdp query ById",
      Description = "Executed MinIdp authentication query by id",
      RequestBody = "{realm: atom, id: string}",
      RequestQueryParameters = new[]{"realm: atom", "id: string", "plain: bool", "ctx: AuthenticationRequestContext" },
      Methods = new[]{"POST: json body", "GET: query params"})]
    [Action(Name = "byid")]
    public async Task<object> GetById(Atom realm, string id, bool plain, AuthenticationRequestContext ctx)
    {
      var store = Store;
      return secure(store, await store.GetByIdAsync(realm, id, ctx), plain);
    }

    [ApiEndpointDoc(
      Title = "MinIdp query BySys",
      Description = "Executed MinIdp authentication query by system auth token",
      RequestBody = "{realm: atom, sysToken: string}",
      RequestQueryParameters = new[] { "realm: atom", "sysToken: string", "plain: bool", "ctx: AuthenticationRequestContext" },
      Methods = new[] { "POST: json body", "GET: query params" })]
    [Action(Name = "bysys")]
    public async Task<object> GetBySys(Atom realm, string sysToken, bool plain, AuthenticationRequestContext ctx)
    {
      var store = Store;
      return secure(store, await store.GetBySysAsync(realm, sysToken, ctx), plain);
    }

    [ApiEndpointDoc(
      Title = "MinIdp query ByUri",
      Description = "Executed MinIdp authentication query by entity Uri",
      RequestBody = "{realm: atom, uri: string}",
      RequestQueryParameters = new[] { "realm: atom", "uri: string", "plain: bool", "ctx: AuthenticationRequestContext" },
      Methods = new[] { "POST: json body", "GET: query params" })]
    [Action(Name = "byuri")]
    public async Task<object> GetByUri(Atom realm, string uri, bool plain, AuthenticationRequestContext ctx = null)
    {
      var store = Store;
      return secure(store, await store.GetByUriAsync(realm, uri, ctx), plain);
    }

    [ApiEndpointDoc(
      Title = "Executes MinIdp management script",
      Description = "Executes MinIdp management script responding with json result object",
      RequestBody = "{source: laconicString}",
      Methods = new []{"POST: json body"},
      ResponseContent = "Json object {OK: bool, ctype: string, data: {}}")]
    [ActionOnPost(Name = "exec"), AcceptsJson]
    public void ExecCommand(string source)
    {
      IConfigSectionNode command = null;

      try
      {
        command = source.NonBlank(nameof(source))
                           .AsLaconicConfig(handling: Data.ConvertErrorHandling.Throw);
      }
      catch(Exception error)
      {
        throw new HTTPStatusException(400, "Bad command syntax", error.ToMessageWithType());
      }

      var callable = (TargetStore as IExternallyCallable).NonNull("Is not {0}".Args(nameof(IExternallyCallable)));
      var handler = callable.GetExternalCallHandler();
      var got = handler.HandleRequest(command);

      if (got == null)
      {
        WorkContext.Response.StatusCode = 400;
        WorkContext.Response.StatusDescription = "Bad request: command not understood";
        WorkContext.Response.WriteJSON(new {OK = false, cmd = command.Name});
        return;
      }

      WorkContext.Response.StatusCode = got.StatusCode;
      WorkContext.Response.StatusDescription = got.StatusDescription;
      WorkContext.Response.WriteJSON(new {OK = true, ctype = got.ContentType, data = got.Content}, Serialization.JSON.JsonWritingOptions.PrettyPrintRowsAsMapASCII);
    }
  }
}
