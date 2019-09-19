using System;
using System.Collections.Generic;
using System.Text;

using Azos.Wave.Mvc;

namespace Azos.IAM.Server.Api
{
  [NoCache]
  [ApiControllerDoc(BaseUri ="/doc", Authentication = "pub")]
  public class Doc : ApiDocController
  {
    protected override ApiDocGenerator MakeDocGenerator()
    {
      var generator = new ApiDocGenerator(App);
      generator.Locations.Add( new ApiDocGenerator.ControllerLocation("Azos.IAM.Server.dll", "*Api*"));
      generator.IgnoreTypePatterns.Add("System.*");
      return generator;
    }

    [Action] public object Index() => new Redirect("toc");
  }
}
