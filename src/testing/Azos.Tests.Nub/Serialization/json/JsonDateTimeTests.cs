using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.Serialization.json
{
  [Runnable]
  public class JsonDateTimeTests
  {
    [Run]
    public void FreeformUnspecified_1()
    {
      var json = "{d: '07/12/1985 12:13:57 PM'}";
      var map = JsonReader.DeserializeDataObject(json) as JsonDataMap;
      Aver.IsNotNull(map);

      Aver.IsTrue( map["d"] is string);

      var got = map["d"].AsDateTime();

      Aver.IsTrue(DateTimeKind.Unspecified == got.Kind);
      Aver.AreEqual(1985, got.Year);
      Aver.AreEqual(7, got.Month);
      Aver.AreEqual(12, got.Day);
      Aver.AreEqual(12, got.Hour);
      Aver.AreEqual(13, got.Minute);
      Aver.AreEqual(57, got.Second);
    }

    [Run]
    public void FreeformUnspecified_2_AdjustUniversal()
    {
      var json = "{d: 'July 12 1985 12:13:57 PM'}";
      var map = JsonReader.DeserializeDataObject(json) as JsonDataMap;
      Aver.IsNotNull(map);

      Aver.IsTrue(map["d"] is string);

      var got = map["d"].AsDateTime(DateTimeStyles.AdjustToUniversal); //option has no effect because there is no AssumeUniversal

      Aver.IsTrue(DateTimeKind.Unspecified == got.Kind);
      Aver.AreEqual(1985, got.Year);
      Aver.AreEqual(7, got.Month);
      Aver.AreEqual(12, got.Day);
      Aver.AreEqual(12, got.Hour);
      Aver.AreEqual(13, got.Minute);
      Aver.AreEqual(57, got.Second);
    }

    [Run]
    public void ISO_UTC_1()
    {
      var json = "{d: '1985-12-31T23:59:53Z'}";
      var map = JsonReader.DeserializeDataObject(json) as JsonDataMap;
      Aver.IsNotNull(map);

      Aver.IsTrue(map["d"] is string);

      var got = map["d"].AsDateTime(DateTimeStyles.AdjustToUniversal); //converts with zone

      got.See();
      Aver.IsTrue(DateTimeKind.Utc == got.Kind);
      Aver.AreEqual(1985, got.Year);
      Aver.AreEqual(12, got.Month);
      Aver.AreEqual(31, got.Day);
      Aver.AreEqual(23, got.Hour);
      Aver.AreEqual(59, got.Minute);
      Aver.AreEqual(53, got.Second);
    }


    [Run]
    public void Doc_DefaultUTC_fromUTC_1()
    {
      var json = "{date: '1985-12-31T23:59:53Z', ndate: null}";
      var doc = JsonReader.ToDoc<DateDoc>(json);
      Aver.IsNotNull(doc);
      doc.See();

      Aver.IsNull(doc.NDate);

      Aver.IsTrue(DateTimeKind.Utc == doc.Date.Kind); //default is UTC
      Aver.AreEqual(1985, doc.Date.Year);
      Aver.AreEqual(12, doc.Date.Month);
      Aver.AreEqual(31, doc.Date.Day);
      Aver.AreEqual(23, doc.Date.Hour);
      Aver.AreEqual(59, doc.Date.Minute);
      Aver.AreEqual(53, doc.Date.Second);
    }

    [Run]
    public void Doc_DefaultUTC_fromUTC_2()
    {
      var json = "{date: '1985-12-31T23:59:53Z', ndate: '1985-12-31T23:59:53-05:00'}";
      var doc = JsonReader.ToDoc<DateDoc>(json);
      Aver.IsNotNull(doc);
      doc.See();

      Aver.IsNotNull(doc.NDate);

      Aver.IsTrue(DateTimeKind.Utc == doc.Date.Kind); //default is UTC
      Aver.AreEqual(1985, doc.Date.Year);
      Aver.AreEqual(12, doc.Date.Month);
      Aver.AreEqual(31, doc.Date.Day);
      Aver.AreEqual(23, doc.Date.Hour);
      Aver.AreEqual(59, doc.Date.Minute);
      Aver.AreEqual(53, doc.Date.Second);

      Aver.IsTrue(DateTimeKind.Utc == doc.NDate.Value.Kind);//second one is local but converted to UTC as asked for
      Aver.AreEqual(1986, doc.NDate.Value.Year);
      Aver.AreEqual(1, doc.NDate.Value.Month);
      Aver.AreEqual(1, doc.NDate.Value.Day);
      Aver.AreEqual(4, doc.NDate.Value.Hour);//offset by +5 hrs
      Aver.AreEqual(59, doc.NDate.Value.Minute);
      Aver.AreEqual(53, doc.NDate.Value.Second);
    }

    [Run]
    public void Doc_LocalDates_fromLocal()
    {
      var json = "{date: '1985-12-31T23:59:53-05:00', ndate: null}";
      var doc = JsonReader.ToDoc<DateDoc>(json, options: new JsonReader.DocReadOptions(JsonReader.DocReadOptions.By.CodeName,null, localDates: true));
      Aver.IsNotNull(doc);
      doc.Date.See("Kind: "+doc.Date.Kind);

      Aver.IsNull(doc.NDate);

      Aver.IsTrue(DateTimeKind.Local == doc.Date.Kind);//asked for local date and got local
      Aver.AreEqual(1985, doc.Date.Year);
      Aver.AreEqual(12, doc.Date.Month);
      Aver.AreEqual(31, doc.Date.Day);
      Aver.AreEqual(23, doc.Date.Hour);
      Aver.AreEqual(59, doc.Date.Minute);
      Aver.AreEqual(53, doc.Date.Second);
    }

    [Run]
    public void Doc_UTCDates_fromLocal_1()
    {
      var json = "{date: '1985-12-31T23:59:53-05:00', ndate: null}"; //notice that date is supplied with -5:00 local timezone
      var doc = JsonReader.ToDoc<DateDoc>(json, options: new JsonReader.DocReadOptions(JsonReader.DocReadOptions.By.CodeName, null, localDates: false));
      Aver.IsNotNull(doc);
      doc.Date.See("Kind: " + doc.Date.Kind);

      Aver.IsNull(doc.NDate);

      Aver.IsTrue(DateTimeKind.Utc == doc.Date.Kind);//asked for UTC
      Aver.AreEqual(1986, doc.Date.Year);
      Aver.AreEqual(1, doc.Date.Month);
      Aver.AreEqual(1, doc.Date.Day);
      Aver.AreEqual(4, doc.Date.Hour);//offset by +5 hrs
      Aver.AreEqual(59, doc.Date.Minute);
      Aver.AreEqual(53, doc.Date.Second);
    }

    [Run]
    public void Doc_UTCDates_fromLocal_2()
    {
      var json = "{date: '1985-12-31T23:59:53-07:30', ndate: null}"; //notice that date is supplied with -7:30 local timezone
      var doc = JsonReader.ToDoc<DateDoc>(json, options: new JsonReader.DocReadOptions(JsonReader.DocReadOptions.By.CodeName, null, localDates: false));
      Aver.IsNotNull(doc);
      doc.Date.See("Kind: " + doc.Date.Kind);

      Aver.IsNull(doc.NDate);

      Aver.IsTrue(DateTimeKind.Utc == doc.Date.Kind);//asked for UTC
      Aver.AreEqual(1986, doc.Date.Year); //day falls on the next year
      Aver.AreEqual(1, doc.Date.Month);
      Aver.AreEqual(1, doc.Date.Day);
      Aver.AreEqual(7, doc.Date.Hour);//offset by +7:30 hrs
      Aver.AreEqual(29, doc.Date.Minute);
      Aver.AreEqual(53, doc.Date.Second);
    }

    [Run]
    public void Doc_UTCDates_fromUTC()
    {
      var json = "{date: '1985-12-31T23:59:53Z', ndate: null}";
      var doc = JsonReader.ToDoc<DateDoc>(json, options: new JsonReader.DocReadOptions(JsonReader.DocReadOptions.By.CodeName, null, localDates: false));
      Aver.IsNotNull(doc);
      doc.Date.See("Kind: " + doc.Date.Kind);

      Aver.IsNull(doc.NDate);

      Aver.IsTrue(DateTimeKind.Utc == doc.Date.Kind);//asked for UTC
      Aver.AreEqual(1985, doc.Date.Year);
      Aver.AreEqual(12, doc.Date.Month);
      Aver.AreEqual(31, doc.Date.Day);
      Aver.AreEqual(23, doc.Date.Hour);
      Aver.AreEqual(59, doc.Date.Minute);
      Aver.AreEqual(53, doc.Date.Second);
    }


    public class DateDoc : TypedDoc
    {
      [Field]public DateTime  Date  { get; set; }
      [Field]public DateTime? NDate { get; set; }
    }
  }
}
