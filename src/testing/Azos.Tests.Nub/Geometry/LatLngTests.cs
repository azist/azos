/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;
using Azos.Geometry;
using Azos.Data;
using Azos.Serialization.JSON;
using Azos.Conf;

namespace Azos.Tests.Nub.Geometry
{
  [Runnable]
  public class LatLngTests
  {
    [Run]
    public void FromDegreeString_ToString()
    {
      var cleveland = new LatLng("41°29'13'', -81°38'26''");

      "Cleveland: {0}".SeeArgs(cleveland);
      Aver.AreEqual("41°29'13'', -81°38'26''", cleveland.ToString());
    }

    [Run]
    public void FromDecimalString_Distance_CLE_LA()
    {
      var cleveland = new LatLng("41.4868145,-81.6406292");
      var losangeles = new LatLng("34.1610243,-117.9465513");

      "Cleveland: {0}".SeeArgs(cleveland);
      "Los Angeles: {0}".SeeArgs(losangeles);

      var dist = cleveland.HaversineEarthDistanceKm(losangeles);

      "Distance: {0} km".SeeArgs(dist);
      Aver.AreEqual(3265, (int)dist);
    }

    [Run]
    public void FromDecimalString_Distance_LA_CLE()
    {
      var cleveland = new LatLng("41.4868145,-81.6406292");
      var losangeles = new LatLng("34.1610243,-117.9465513");

      Conout.WriteLine(cleveland);
      Conout.WriteLine(losangeles);

      var dist = losangeles.HaversineEarthDistanceKm(cleveland);

      Conout.WriteLine(dist);
      Aver.AreEqual(3265, (int)dist);
    }

    [Run]
    public void FromDegreeString_Distance_CLE_LA()
    {
      var cleveland = new LatLng("41°29'13'', -81°38'26''");
      var losangeles = new LatLng("34°9'40'', -117°56'48''");

      Conout.WriteLine(cleveland);
      Conout.WriteLine(losangeles);

      var dist = cleveland.HaversineEarthDistanceKm(losangeles);

      Conout.WriteLine(dist);
      Aver.AreEqual(3265, (int)dist);
    }

    [Run]
    public void FromDecimalString_Distance_CLE_MOSCOW()
    {
      var cleveland = new LatLng("41.4868145,-81.6406292");
      var moscow = new LatLng("55.7530361,37.6217305");

      Conout.WriteLine(cleveland);
      Conout.WriteLine(moscow);

      var dist = cleveland.HaversineEarthDistanceKm(moscow);

      Conout.WriteLine(dist);
      Aver.AreEqual(7786, (int)dist);
    }

    [Run]
    public void FromDecimalString_Distance_MOSCOW_CLE()
    {
      var cleveland = new LatLng("41.4868145,-81.6406292");
      var moscow = new LatLng("55.7530361,37.6217305");

      Conout.WriteLine(cleveland);
      Conout.WriteLine(moscow);

      var dist = moscow.HaversineEarthDistanceKm(cleveland);

      Conout.WriteLine(dist);
      Aver.AreEqual(7786, (int)dist);
    }

    [Run]
    public void FromDegreeString_Distance_CLE_MOSCOW()
    {
      var cleveland = new LatLng("41°29'13'', -81°38'26''");
      var moscow = new LatLng("55°45'11'', 37°37'18''");

      Conout.WriteLine(cleveland);
      Conout.WriteLine(moscow);

      var dist = cleveland.HaversineEarthDistanceKm(moscow);

      Conout.WriteLine(dist);
      Aver.AreEqual(7786, (int)dist);
    }

    [Run]
    public void FromDecimalString_Distance_MELBOURNE_CLE()
    {
      var cleveland = new LatLng("41.4868145,-81.6406292");
      var melbourne = new LatLng("-37.5210205,144.7461265");

      Conout.WriteLine(cleveland);
      Conout.WriteLine(melbourne);

      var dist = melbourne.HaversineEarthDistanceKm(cleveland);

      Conout.WriteLine(dist);
      Aver.AreEqual(16058, (int)dist);
    }

    [Run]
    public void FromDecimalString_Distance_CLE_MELBOURNE()
    {
      var cleveland = new LatLng("41.4868145,-81.6406292");
      var melbourne = new LatLng("-37.5210205,144.7461265");

      Conout.WriteLine(cleveland);
      Conout.WriteLine(melbourne);

      var dist = cleveland.HaversineEarthDistanceKm(melbourne);

      Conout.WriteLine(dist);
      Aver.AreEqual(16058, (int)dist);
    }

    [Run]
    public void FromDegreeString_Distance_CLE_MELBOURNE()
    {
      var cleveland = new LatLng("41°29'13'', -81°38'26''");
      var melbourne = new LatLng("-37°31'16'', 144°44'46''");

      Conout.WriteLine(cleveland);
      Conout.WriteLine(melbourne);

      var dist = cleveland.HaversineEarthDistanceKm(melbourne);

      Conout.WriteLine(dist);
      Aver.AreEqual(16058, (int)dist);
    }

    [Run]
    public void Equality1()
    {
      var cle1 = new LatLng("41°29'13'', -81°38'26''");
      var cle2 = new LatLng("41°29'13'', -81°38'26''");

      Aver.AreEqual(cle1, cle2);
    }

    [Run]
    public void Equality2()
    {
      var loc1 = new LatLng("44°13'51'', 12°32'4''");
      var loc2 = new LatLng("44°13'51'', 12°32'4''");

      Aver.AreEqual(loc1, loc2);
    }

    class _doc : TypedDoc
    {
      [Field, Config] public LatLng Location { get; set; }
    }

    [Run]
    public void Equality3_Documents()
    {
      var doc1 = new _doc{Location = new LatLng("44°13'51'', 12°32'4''")};
      var doc2 = new _doc{Location = new LatLng("44°13'51'', 12°32'4''")};

      doc1.AverNoDiff(doc2);
    }

    [Run]
    public void Equality4_Document_JSON_Roundtrip01()
    {
      var doc1 = new _doc { Location = new LatLng("44°13'51'', 12°32'4''") };

      var json = doc1.ToJson();
      json.See();

      var doc2 = JsonReader.ToDoc<_doc>(json);

      doc1.AverNoDiff(doc2);
    }

    [Run]
    public void Equality4_Document_JSON_Roundtrip02()
    {
      var doc1 = new _doc { Location = new LatLng("44.23091016, 12.5345640") };

      var json = doc1.ToJson();
      json.See();

      var doc2 = JsonReader.ToDoc<_doc>(json);

      //doc2.CompareTo(doc1).Differences.See();

      doc1.AverNoDiff(doc2);
    }

    [Run]
    public void Equality4_Document_JSON_Roundtrip03_Marshalling()
    {
      var doc1 = new _doc { Location = new LatLng("44.23091016, 12.5345640") };

      var json = doc1.ToJson(new JsonWritingOptions(JsonWritingOptions.PrettyPrintRowsAsMap){ Purpose = JsonSerializationPurpose.Marshalling});
      json.See();

      var doc2 = JsonReader.ToDoc<_doc>(json);

      //doc2.CompareTo(doc1).Differences.See();

      doc1.AverNoDiff(doc2);
    }

    [Run]
    public void Equality5_Document_Config_Roundtrip()
    {
      var doc1 = new _doc { Location = new LatLng("44°13'51'', 12°32'4''") };
      doc1.See();
      "{0}  {1}".SeeArgs(doc1.Location.Lat, doc1.Location.Lng);

      var cfg = Azos.Conf.Configuration.NewEmptyRoot();

      var data = doc1.PersistConfiguration(cfg, "data");

      cfg.See();

      var doc2 = new _doc();
      doc2.Configure(data);
      doc2.See();
      "{0}  {1}".SeeArgs(doc2.Location.Lat, doc2.Location.Lng);

      //"Equals whole: {0}".SeeArgs(doc1.Location == doc2.Location);
      //"Equals name: {0}".SeeArgs(doc1.Location.Name == doc2.Location.Name);
      //"Equals lat: {0}".SeeArgs(doc1.Location.Lat == doc2.Location.Lat);
      //"Equals lng: {0}".SeeArgs(doc1.Location.Lng == doc2.Location.Lng);

     // doc2.CompareTo(doc1).Differences.See();

      doc1.AverNoDiff(doc2);
    }

  }
}