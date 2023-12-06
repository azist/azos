using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.Tests.Nub.Time
{
  [Runnable]
  public class UtcHistoryTests
  {
    public class Perzon : TypedDoc
    {
      [Field] public string Name{ get; set;}
    }

    public class Perzons : TypedDoc
    {
      [Field(required: true)]
      public UtcHistory<Perzon> P1 { get; set; }

      [Field(minLength: 2, maxLength: 4)]
      public UtcHistory<Perzon> P2 { get; set; }
    }


    [Run]
    public void FullCycle()
    {
      var sut = new UtcHistory<Perzon>();
      Aver.AreEqual(0, sut.Count);

      sut.Add(new DateTime(1980, 1, 1, 13, 00, 00, DateTimeKind.Utc), new Perzon{ Name = "Snakes"});
      Aver.AreEqual(1, sut.Count);

      Aver.IsFalse(sut[new DateTime(1979, 12, 31, 0, 0, 0, DateTimeKind.Utc)].Assigned);
      Aver.IsTrue(sut[new DateTime(1982, 12, 31, 0, 0, 0, DateTimeKind.Utc)].Assigned);
      Aver.AreEqual("Snakes", sut[new DateTime(1982, 12, 31, 0, 0, 0, DateTimeKind.Utc)].Data.Name);

      sut.Add(new DateTime(1990, 1, 1, 13, 00, 00, DateTimeKind.Utc), new Perzon { Name = "Gates" });
      Aver.AreEqual(2, sut.Count);

      Aver.IsFalse(sut[new DateTime(1979, 12, 31, 0, 0, 0, DateTimeKind.Utc)].Assigned);
      Aver.IsTrue(sut[new DateTime(1982, 12, 31, 0, 0, 0, DateTimeKind.Utc)].Assigned);
      Aver.AreEqual("Snakes", sut[new DateTime(1982, 12, 31, 0, 0, 0, DateTimeKind.Utc)].Data.Name);
      Aver.AreEqual("Gates", sut[new DateTime(1995, 12, 31, 0, 0, 0, DateTimeKind.Utc)].Data.Name);

      Aver.IsTrue(sut.CheckRequired(null));
      Aver.IsTrue(sut.CheckMinLength(null, 2));
      Aver.IsFalse(sut.CheckMinLength(null, 3));

      Aver.IsTrue(sut.CheckMaxLength(null, 2));
      Aver.IsFalse(sut.CheckMaxLength(null, 1));

      var json = sut.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);

      json.See();

      var got = new UtcHistory<Perzon>();
      Aver.IsTrue(((IJsonReadable)got).ReadAsJson(json.JsonToDataObject(), false, null).match);
      Aver.AreEqual(2, sut.Count);

      Aver.IsFalse(sut[new DateTime(1979, 12, 31, 0, 0, 0, DateTimeKind.Utc)].Assigned);
      Aver.IsTrue(sut[new DateTime(1982, 12, 31, 0, 0, 0, DateTimeKind.Utc)].Assigned);
      Aver.AreEqual("Snakes", sut[new DateTime(1982, 12, 31, 0, 0, 0, DateTimeKind.Utc)].Data.Name);
      Aver.AreEqual("Gates", sut[new DateTime(1995, 12, 31, 0, 0, 0, DateTimeKind.Utc)].Data.Name);
    }

    [Run]
    public void TypedDocs()
    {

    }

  }
}
