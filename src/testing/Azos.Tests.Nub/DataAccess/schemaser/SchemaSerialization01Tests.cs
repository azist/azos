/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Azos.Data;
using Azos.Data.Adlib;
using Azos.Geometry;
using Azos.Scripting;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class SchemaSerialization01Tests
  {
    [Bix("5aaa3f66-5593-460f-bcd7-0d480310aaea")]
    [Schema(Description = "Test document A")]
    [Schema(targetName: "oldShit", Description = "Test document A in the old system")]
    public class DocA : TypedDoc
    {
      [Field(required: true, Description = "Patient first name", MetadataContent = "a=1 b=true")]
      [Field("rabel1", null, MinLength = 5, MetadataContent ="c=-9 b=false")]
      [Field(targetName: "ORACLE", MinLength = 5, MetadataContent = "c=-9 b=false")]
      public string FirstName{ get; set; }


      [Field]
      public List<DocA> Subdocuments{ get; set; }

      [Field]
      public JsonDataMap Props { get; set; }

      [Field]
      public GDID[] ExtraGdids { get; set; }

      [Field]
      public Atom[] Flags { get; set; }

      [Field]
      public List<LatLng> Locations { get; set; }

      [Field(required: true)]
      public Relative Father { get; set; }

      [Field(required: true)]
      public Relative Mother { get; set; }
    }

    [Bix("5aaa3f66-5593-460f-bcd7-0d480310aaff")]
    public class Relative : TypedDoc
    {
      [Field(required: true, Description = "First name")]
      public string FirstName { get; set; }

      [Field(required: true, maxLength: 16, Description = "Lastname", ValueList = "Smith: Smithonian farter;Wallace: Boltzman farting skew")]
      public string LastName { get; set; }

      [Field(Description = "DOB")]
      public DateTime? DOB { get; set; }
    }


    [Run]
    public void Case01()
    {
      var orig = Schema.GetForTypedDoc<DocA>();
      var map1 = SchemaSerializer.Serialize(new SchemaSerializer.SerCtx(orig), "BarMarLey");
      map1.See();

      var got = SchemaSerializer.Deserialize(new SchemaSerializer.DeserCtx(map1));
      var map2 = SchemaSerializer.Serialize(new SchemaSerializer.SerCtx(got), "BarMarLey");
      map2.See();
    }

    [Run]
    public void FullCycle()
    {
      var orig = Schema.GetForTypedDoc<DocA>();
      var map1 = SchemaSerializer.Serialize(new SchemaSerializer.SerCtx(orig), "form1");
      var json = map1.ToJson();

      json.See("WIRE JSON ========================= ");

      var schema2 = SchemaSerializer.Deserialize(new SchemaSerializer.DeserCtx(json.JsonToDataObject().ExpectJsonDataMap()));

      var form = new DynamicDoc(schema2);

      var father = new DynamicDoc(form.Schema["Father"].ComplexTypeSchema);
      form["Father"] = father;

      father["LastName"] = "Smith the fart";

      var vstate = form.Validate(new ValidState("*", ValidErrorMode.Batch, 1000));
      new WrappedExceptionData(vstate.Error).See();
    }
  }
}
