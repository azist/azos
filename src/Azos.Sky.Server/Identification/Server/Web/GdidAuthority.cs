/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using Azos.Apps.Injection;
using Azos.Security;
using Azos.Sky.Contracts;
using Azos.Wave.Mvc;

namespace Azos.Sky.Identification.Server.Web
{
  [NoCache]
  [AuthenticatedUserPermission]
  [ApiControllerDoc(
    BaseUri = "/es/gdidauthority",
    Connection = "default/keep alive",
    Title = "Gdid Generation Authority",
    Authentication = "Token/Default",
    Description = "Provides API for calling Gdid Authority",
    TypeSchemas = new[]{typeof(AuthenticatedUserPermission), typeof(GdidBlock) }
  )]
  [Release(ReleaseType.Release, 2020, 08, 05, "Initial Release", Description = "Gdid Authority Web Server")]
  public class GdidAuthority : ApiProtocolController
  {
    [Inject] IGdidAuthorityModule m_Authority;

    [ApiEndpointDoc(Title = "Gdid Block Allocator",
                    Uri = "block",
                    Description = "Allocates the next `{@GdidBlock}` for the supplied scope/sequence",
                    Methods = new[] { "POST - post {scopeName, sequenceName, blockSize, vicinity} to allocate the next `{@GdidBlock}`" },
                    RequestHeaders = new[] { API_DOC_HDR_ACCEPT_JSON },
                    ResponseHeaders = new[] { API_DOC_HDR_NO_CACHE },
                    RequestBody = "JSON representation of {scopeName, sequenceName, blockSize, vicinity}",
                    ResponseContent = "Json of `{@GdidBlock}`")]
    [ActionOnPost(Name = "block"), AcceptsJson]
    public object Block(string scopeName, string sequenceName, int blockSize, ulong? vicinity = ulong.MaxValue)
     => GetLogicResult(m_Authority.AllocateBlock(scopeName, sequenceName, blockSize, vicinity));

  }
}
