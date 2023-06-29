/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Security.Fabric;
using Azos.Wave.Mvc;

namespace Azos.Sky.FileGateway.Server.Web
{
  [NoCache]
  [FabricPermission]
  [ApiControllerDoc(
    BaseUri = "/file/gateway",
    Connection = "default/keep alive",
    Title = "File Gateway",
    Authentication = "Token/Default",
    Description = "Provides REST API for working with remote files",
    TypeSchemas = new[]{typeof(FabricPermission) }
  )]
  [Release(ReleaseType.Preview, 2023, 06, 29, "Initial Release", Description = "First release of API")]
  public class Gateway : ApiProtocolController
  {

  }
}
