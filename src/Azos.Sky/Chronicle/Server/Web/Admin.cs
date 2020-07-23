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
  [ChroniclePermission(AccessLevel.ADVANCED)]
  [ApiControllerDoc(
    BaseUri = "/chronicle/admin",
    Connection = "default/keep alive",
    Title = "Chronicle Admin",
    Authentication = "Token/Default",
    Description = "Provides REST API for performing chronicle admin tasks tbd..",
    TypeSchemas = new[]{typeof(ChroniclePermission) }
  )]
  //[Release(ReleaseType.Preview, 2020, 07, 05, "Initial Release", Description = "Preview release of API")]
  public class Admin : ApiProtocolController
  {
    [Action]
    public object Index() => "Admin space";


    //For future:
    //we can take admin command object and route it to service for administration
  }
}
