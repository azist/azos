using System;
using System.Collections.Generic;
using System.Text;

using Azos;
using Azos.Wave.Mvc;

namespace WaveTestSite.Controllers
{
  /// <summary>
  /// Generates documentation for this site
  /// </summary>
  [Release(ReleaseType.Preview, 2019, 08, 01, "Initial testing")]
  [ApiControllerDoc(BaseUri = "/mvc/testapidoc", Title = "TestApiDoc", Description = "This controller is used for testing")]
  public class TestApiDoc : ApiDocController
  {
    protected override ApiDocGenerator MakeDocGenerator()
    {
      var gen = new ApiDocGenerator(App);
      gen.Locations.Add(new ApiDocGenerator.ControllerLocation("TestBusinessLogic.dll", "WaveTestSite.Cont*"));
      return gen;
    }

    [Action]
    public object Index(string uriPattern = null) => new Redirect("/mvc/testapidoc/toc");

    [Release(ReleaseType.Preview, 2019, 08, 01, "Throw", Description = "This method was released to throw exceptions")]
    [Action(Name ="throw"), ApiEndpointDoc(Description ="Used to throw fake exception to test error pages")]
    public void Throw()
    {
      throw new Azos.AzosException("The king was drunk");
    }
  }
}
