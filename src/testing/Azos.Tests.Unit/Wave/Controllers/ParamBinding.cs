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
    public object EchoMixMap(string a, int b, bool c) => new { a, b, c, got = WorkContext.MatchedVars };

    [Action]
    public object EchoModelA(ModelA got) => got;

    [Action] //AZ #520
    public object EchoMixModelA(string id, string another, ModelA model) => new { id, another, model };

    [Action]
    public object EchoBuffer(byte[] buffer) => new BinaryContent(buffer);


    [Action]
    public object EchoVariousParams(GDID gd, Guid gu, Atom a, EntityId e, DateTime dt, decimal m, double d, bool b, long li, string s)
      => new { gd, gu, a, e, dt, m, d, b, li, s};
  }
}
