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
  public class SchemaSerialization02Tests
  {
    [Bix("74e83145-c432-4761-9fc4-2870fd8150e2")]
    [Schema(Description = "Test document A")]
    public class RoomForm : TypedDoc
    {
      [Field]
      public string Name{ get; set; }

      [Field]
      public int? Size{ get; set; }

      [Field]
      public List<WallSection> Walls { get; set; }

    }

    [Bix("11e6d16c-ba5d-47c3-8ed3-4f2fa980dd90")]
    public class WallSection : TypedDoc
    {
      [Field]
      public string Name { get; set; }

      [Field]
      public int? Width { get; set; }

      [Field]
      public int? Height { get; set; }

      [Field]
      public List<WindowSection> Windows { get; set; }
    }

    [Bix("72060172-29f3-47d2-9515-4632cc6936cc")]
    public class WindowSection : TypedDoc
    {
      [Field]
      public int? Seq { get; set; }

      [Field]
      public int? Width { get; set; }

      [Field]
      public int? Height { get; set; }
    }


    [Run]
    public void Case01()
    {
      var orig = Schema.GetForTypedDoc<RoomForm>();
      var map1 = SchemaSerializer.Serialize(new SchemaSerializer.SerCtx(orig), "RoomForm");
      map1.See();

      var got = SchemaSerializer.Deserialize(new SchemaSerializer.DeserCtx(map1));
      var map2 = SchemaSerializer.Serialize(new SchemaSerializer.SerCtx(got), "RoomForm");
      map2.See();
      //got.See();
    }

    [Run]
    public void Case02()
    {
      var orig = Schema.GetForTypedDoc<RoomForm>();
      var map1 = SchemaSerializer.Serialize(new SchemaSerializer.SerCtx(orig), "RoomForm");

      var schema = SchemaSerializer.Deserialize(new SchemaSerializer.DeserCtx(map1));
      var schWall = schema["Walls"].ComplexTypeSchema;
      var schWindow = schWall["Windows"].ComplexTypeSchema;

      var form = new DynamicDoc(schema);
      form["Name"] = "Dining";
      form["Size"] = 47;
      var walls = new List<DynamicDoc>();
      var wall = new DynamicDoc(schWall);
      walls.Add(wall);
      wall["Name"] =  "A";
      wall["Width"] = 2120;
      wall["Height"] = 240;
      var wins = new List<DynamicDoc>();
      var win = new DynamicDoc(schWindow);
      win["Seq"] = "1";
      win["Width"] = 100;
      win["Height"] = 179;
      wins.Add(win);
      win = new DynamicDoc(schWindow);
      win["Seq"] = "2";
      win["Width"] = 101;
      win["Height"] = 180;
      wins.Add(win);
      wall["Windows"] = wins.ToArray();

      form["Walls"] = walls.ToArray();
      form.See();

      var json = form.ToJson();
      var form2 = json.JsonToDataObject() as JsonDataMap;
      Aver.IsNotNull(form2);

      Aver.AreEqual("Dining", form2["Name"].AsString());
      Aver.AreEqual(47, form2["Size"].AsInt());
      var walls2 = form2["Walls"] as JsonDataArray;
      Aver.IsNotNull(walls2);
      Aver.AreEqual(1, walls2.Count);
      var windows2 = (walls2[0] as JsonDataMap)["Windows"] as JsonDataArray;
      var win1 = windows2[0] as JsonDataMap;
      var win2 = windows2[1] as JsonDataMap;
      Aver.IsNotNull(win1);
      Aver.IsNotNull(win2);
      Aver.AreEqual(1, win1["Seq"].AsInt());
      Aver.AreEqual(100, win1["Width"].AsInt());
      Aver.AreEqual(179, win1["Height"].AsInt());

      Aver.AreEqual(2, win2["Seq"].AsInt());
      Aver.AreEqual(101, win2["Width"].AsInt());
      Aver.AreEqual(180, win2["Height"].AsInt());
    }
  }
}
