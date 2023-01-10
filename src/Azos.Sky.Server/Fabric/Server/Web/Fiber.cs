/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Log;
using Azos.Platform;
using Azos.Security.Fabric;
using Azos.Wave.Mvc;
using Azos.Web;

namespace Azos.Sky.Fabric.Server.Web
{
  [NoCache]
  [FabricPermission]
  [ApiControllerDoc(
    BaseUri = "/fabric/fiber",
    Connection = "default/keep alive",
    Title = "Fiber manager",
    Authentication = "Token/Default",
    Description = "Provides REST API for managing fabric fibers",
    TypeSchemas = new[]{typeof(FabricPermission) }
  )]
  [Release(ReleaseType.Preview, 2023, 01, 10, "Initial Release", Description = "Preview release of API")]
  public class Fiber : ApiProtocolController
  {

  }
}
