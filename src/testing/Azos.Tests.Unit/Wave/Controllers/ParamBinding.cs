using System;

using Azos.Web;
using Azos.Data;
using Azos.Wave.Mvc;
using Azos.Serialization.JSON;

namespace Azos.Tests.Unit.Wave.Controllers
{

  public class ModelA : TypedDoc
  {
    [Field(required: true, key: true)]
    public string ID { get;set;}

    [Field(required: true)]
    public string Name { get; set; }

    [Field]
    public decimal? Pay { get; set; }

    [Field(required: true)]
    public DateTime? DOB { get; set; }
  }


  public class ParamBinding : Controller
  {
    [Action]
    public object EchoMap(JsonDataMap got) => got;

    [Action]
    public object EchoParams(string a, int b, bool c) => new { a, b, c };

    [Action]
    public object EchoParamsWithDefaults(string a, int b=127, bool c=true) => new { a, b, c };

    [Action]
    public object EchoMixMap(string a, int b, bool c, JsonDataMap got) => new { a, b, c , got};

    [Action]
    public object EchoModelA(ModelA got) => got;

    [Action]
    public object EchoBuffer(byte[] buffer) => new BinaryContent(buffer);

  }
}
