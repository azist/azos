using System;
using System.Collections.Generic;
using System.Linq;
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

    private static DateTime utc(int y, int m, int d, int h = 0) => new DateTime(y, m, d, h, 0, 0, DateTimeKind.Utc);

    [Run]
    public void FullCycle()
    {
      var sut = new UtcHistory<Perzon>();
      Aver.AreEqual(0, sut.Count);

      sut.Add(utc(1980, 1, 1, 13), new Perzon{ Name = "Snakes"});
      Aver.AreEqual(1, sut.Count);

      Aver.IsFalse(sut[utc(1979, 12, 31)].Assigned);
      Aver.IsTrue(sut[utc(1982, 12, 31)].Assigned);
      Aver.AreEqual("Snakes", sut[utc(1982, 12, 31)].Data.Name);

      sut.Add(new DateTime(1990, 1, 1, 13, 00, 00, DateTimeKind.Utc), new Perzon { Name = "Gates" });
      Aver.AreEqual(2, sut.Count);

      Aver.IsFalse(sut[utc(1979, 12, 31)].Assigned);
      Aver.IsTrue(sut[utc(1982, 12, 31)].Assigned);
      Aver.AreEqual("Snakes", sut[utc(1982, 12, 31)].Data.Name);
      Aver.AreEqual("Gates", sut[utc(1995, 12, 31)].Data.Name);

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

      Aver.IsFalse(sut[utc(1979, 12, 31)].Assigned);
      Aver.IsTrue(sut[utc(1982, 12, 31)].Assigned);
      Aver.AreEqual("Snakes", sut[utc(1982, 12, 31)].Data.Name);
      Aver.AreEqual("Gates", sut[utc(1995, 12, 31)].Data.Name);
    }

    [Run]
    public void TypedDocs_Validation()
    {
      var orig = new Perzons();
      orig.P1 = new UtcHistory<Perzon>();
      orig.P2 = new UtcHistory<Perzon>();


      orig.P2.Add(utc(2010, 7, 4), new Perzon { Name = "Jul 4" });

      var ve = orig.Validate(new ValidState(null, ValidErrorMode.Batch)).Error as ValidationBatchException;
      Aver.AreEqual(2, ve.Batch.Count);
      ve.SeeError();

      orig.P1.Add(utc(2000, 6, 5), new Perzon{ Name = "June 6th" });
      orig.P1.Add(utc(2000, 4, 18), new Perzon { Name = "April 18th" });
      orig.P1.Add(utc(2000, 9, 23), new Perzon { Name = "Sep 23rd" });

      var ve2 = orig.Validate(new ValidState(null, ValidErrorMode.Batch)).Error as FieldValidationException;
      Aver.AreEqual("P2", ve2.FieldName);
      ve2.SeeError();

      orig.P2.Add(utc(2011, 7, 4), new Perzon { Name = "2011 Jul 4" });

      var ve3 = orig.Validate(new ValidState(null, ValidErrorMode.Batch)).Error;
      Aver.IsNull(ve3);
    }

    [Run]
    public void TypedDocs_JsonFullCycle()
    {
      void checkInvariant(Perzons doc)
      {
        Aver.IsNotNull(doc.P1);
        Aver.IsNotNull(doc.P2);

        Aver.AreEqual(3, doc.P1.Count);
        Aver.AreEqual(2, doc.P2.Count);

        Aver.AreEqual("April 18th 2000", doc.P1.First().Data.Name);
        Aver.AreEqual("2000 Sep 23rd", doc.P1.Last().Data.Name);

        Aver.AreEqual("2000 Sep 23rd", doc.P1[utc(2000, 10, 19)].Data.Name);
        Aver.AreEqual("2000 Sep 23rd", doc.P1[utc(2099, 11, 03)].Data.Name);

        Aver.AreEqual("April 18th 2000", doc.P1[utc(2000, 05, 15)].Data.Name);
        Aver.AreEqual("June 5th 2000", doc.P1[utc(2000, 06, 05)].Data.Name);
        Aver.AreEqual("April 18th 2000", doc.P1[utc(2000, 06, 04)].Data.Name);
      }

      var orig = new Perzons();
      orig.P1 = new UtcHistory<Perzon>();
      orig.P2 = new UtcHistory<Perzon>();


      orig.P1.Add(utc(2000, 6, 5), new Perzon { Name = "June 5th 2000" });
      orig.P1.Add(utc(2000, 4, 18), new Perzon { Name = "April 18th 2000" });
      orig.P1.Add(utc(2000, 9, 23), new Perzon { Name = "2000 Sep 23rd" });

      orig.P2.Add(utc(2011, 7, 4), new Perzon { Name = "Jul 4 2011" });
      orig.P2.Add(utc(2010, 7, 4), new Perzon { Name = "Jul 4 2010" });

      checkInvariant(orig);

      var json = orig.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);
      json.See();

      var got = JsonReader.ToDoc<Perzons>(json);
      checkInvariant(got);

      orig.AverNoDiff(got);
    }


  }
}
