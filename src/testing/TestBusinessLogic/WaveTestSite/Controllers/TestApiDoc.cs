using System;
using System.Collections.Generic;
using System.Text;

using Azos.Wave.Mvc;

namespace WaveTestSite.Controllers
{
  /// <summary>
  /// Generates documentation for this site
  /// </summary>
  public class TestApiDoc : ApiDocController
  {
    protected override ApiDocGenerator MakeDocGenerator()
    {
      var gen = new ApiDocGenerator(App);
      gen.Locations.Add(new ApiDocGenerator.ControllerLocation("TestBusinessLogic.dll", "WaveTestSite.Cont*"));
      return gen;
    }

    [Action]
    public void Throw()
    {
      throw new Azos.AzosException("Zar byl piyan");
    }
  }
}
