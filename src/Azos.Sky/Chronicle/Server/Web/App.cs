/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Log;
using Azos.Security;
using Azos.Sky.Security.Permissions.Chronicle;
using Azos.Wave.Mvc;

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

    //we need to know the type of View that
    //implements visualization and return it
    //it can be in a different assembly (the view)

    [Action]
    public object Index() => "<h1>Chronicle App</h1>";

  }
}
