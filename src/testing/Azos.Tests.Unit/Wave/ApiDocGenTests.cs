using System;
using System.Collections.Generic;
using System.Text;

using Azos.Scripting;
using Azos.Wave.Mvc;

namespace Azos.Tests.Unit.Wave
{
  [Runnable]
  public class ApiDocGenTests
  {
    [Run]
    public void Gen1()
    {
      var gen = new ApiDocGenerator();
      gen.Locations.Add(new ApiDocGenerator.ControllerLocation("Azos.Tests.Unit.dll", "Azos.Tests.Unit.Wave*"));

      var got = gen.Generate();

      Console.WriteLine( got.ToLaconicString(CodeAnalysis.Laconfig.LaconfigWritingOptions.PrettyPrint) );
    }
  }


  [ApiControllerDoc(Title ="TestBase", BaseUri = "/test", RequestBody ="json or form url encoded", ResponseHeaders =new[]{"Cache: no-cache"})]
  public class TestController : Controller
  {
    [ApiEndpointDoc(Title="Get list schema", Methods =new[]{ "GET: Gets the schema"}, TypeSchemas = new[] { typeof(TestController) })]
    [Action(Name ="list")]
    public object ListGet()
    {
      return null;
    }

    [ApiEndpointDoc(
      Uri="manual-list",
      Title = "Post filter content returning filtered data list",
      Methods = new[] { "POST: post filter body and generates json with data" },
      TypeSchemas = new[] { typeof(TestController) }
      )]
    [Action(Name = "list")]
    public object ListPost()
    {
      return null;
    }
  }


}
