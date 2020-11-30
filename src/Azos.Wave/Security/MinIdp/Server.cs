/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Wave.Mvc;

namespace Azos.Security.MinIdp
{
  /// <summary>
  /// Controller for serving IMinIdpStore remotely
  /// </summary>
  [NoCache]

  //Do we need any permissions here at all??
  //[IdpPermission] <---- do we need this permission?? TBD
  [AuthenticatedUserPermission]
  [ApiControllerDoc(
    BaseUri = "/minidp/server",
    Connection = "default/keep alive",
    Title = "MinIdp Server",
    Authentication = "Token/Default",
    Description = "Provides API for accessing MinIdp store remotely",
    TypeSchemas = new[] { typeof(AuthenticatedUserPermission) }
  )]
  [Release(ReleaseType.Preview, 2020, 11, 29, "Initial Release", Description = "Preview release of API")]
  public sealed class Server : ApiProtocolController
  {
    private MinIdpSecurityManager Secman
     => (App.SecurityManager as MinIdpSecurityManager).NonNull("App.SecMan is not {0}".Args(nameof(MinIdpSecurityManager)));

    [Action]
    public async Task<object> GetByIdAsync(Atom realm, string id)
     => this.GetLogicResult(await Secman.Store.GetByIdAsync(realm, id));

    [Action]
    public async Task<object> GetBySysAsync(Atom realm, string sysToken)
    => this.GetLogicResult(await Secman.Store.GetBySysAsync(realm, sysToken));

    [Action]
    public async Task<object> GetByUriAsync(Atom realm, string uri)
    => this.GetLogicResult(await Secman.Store.GetByUriAsync(realm, uri));
  }
}
