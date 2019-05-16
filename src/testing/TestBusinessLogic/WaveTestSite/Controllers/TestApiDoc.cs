using System;
using System.Collections.Generic;
using System.Text;

using Azos.Wave.Mvc;

namespace WaveTestSite.Controllers
{
  /// <summary>
  /// Generates documentation for this site
  /// </summary>
  [ApiControllerDoc(BaseUri = "/mvc/testapidoc", Title = "TestApiDoc", Description = "This controller is used for testing")]
  public class TestApiDoc : ApiDocController
  {
    protected override ApiDocGenerator MakeDocGenerator()
    {
      var gen = new ApiDocGenerator(App);
      gen.Locations.Add(new ApiDocGenerator.ControllerLocation("TestBusinessLogic.dll", "WaveTestSite.Cont*"));
      return gen;
    }

    [Action(Name ="throw"), ApiEndpointDoc(Description ="Used to throw fake exception to test error pages")]
    public void Throw()
    {
      throw new Azos.AzosException("The king was drunk");
    }
  }
}
