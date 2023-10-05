/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Reflection;

using Azos.Data;
using Azos.Data.Business;
using Azos.Scripting;
using Azos.Security;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.Tests.Nub.Serialization
{
  /// <summary>
  /// The point of these tests is to deserialize from JsonDataMap which was gotten with type hints, the values of int, EntityId, Gdid etc, not strings.
  /// The bug #891 was about inability to bind data doc from a map with PRESERVED (typehinted) values, so EntityId failed to read into EntityId?
  /// </summary>
  [Runnable]
  public class JsonDocNullableFieldTests
  {
    public class DocWithNullables : AmorphousTypedDoc
    {
      public override bool AmorphousDataEnabled => true;

      [Field] public int? Int1 { get; set; }
      [Field] public int Int2 { get; set; }

      [Field] public decimal? Decimal1 { get; set; }
      [Field] public decimal Decimal2 { get; set; }

      [Field] public DateTime? DateTime1 { get; set; }
      [Field] public DateTime DateTime2 { get; set; }

      [Field] public EntityId? EntityId1 { get; set; }
      [Field] public EntityId EntityId2 { get; set; }

      [Field] public Atom? Atom1 { get; set; }
      [Field] public Atom Atom2 { get; set; }

      [Field] public GDID? GDID1 { get; set; }
      [Field] public GDID GDID2 { get; set; }

      [Field] public RGDID? RGDID1 { get; set; }
      [Field] public RGDID RGDID2 { get; set; }


      [Field] public DateRange? ValidSpanUtc { get; set; }
      [Field] public EntityId? OrgUnit { get; set; }
    }




    [Run]
    public void Test01_string()
    {
      var json = @"{""ValidSpanUtc"": {start: ""1/1/2001"", end: ""12/31/2029""}, ""OrgUnit"": ""path@sys::addr""}";

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.IsNotNull(got);
      Aver.IsNotNull(got.ValidSpanUtc);
      Aver.IsNotNull(got.OrgUnit);

      Aver.AreEqual(2029, got.ValidSpanUtc.Value.End.Value.Year);
      Aver.AreEqual("addr", got.OrgUnit.Value.Address);

      Aver.IsFalse(got.HasAmorphousData);
      got.See();
    }

    [Run]
    public void Test01_map_01()
    {
      var json = new JsonDataMap()
      {
        { "ValidSpanUtc", new JsonDataMap(){{"Start","1/1/2001"},{"end","12/31/2029"}}},
        { "OrgUnit", EntityId.Parse("path@sys::addr")}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.IsNotNull(got);
      Aver.IsNotNull(got.ValidSpanUtc);
      Aver.IsNotNull(got.OrgUnit);

      Aver.AreEqual(2029, got.ValidSpanUtc.Value.End.Value.Year);
      Aver.AreEqual("addr", got.OrgUnit.Value.Address);

      Aver.IsFalse(got.HasAmorphousData);
      got.See();
    }

    [Run]
    public void Test01_map_02()
    {
      var json = new JsonDataMap()
      {
        { "ValidSpanUtc", new DateRange(DateTime.Parse("1/1/2001"), DateTime.Parse("12/31/2029"))},
        { "OrgUnit", EntityId.Parse("path@sys::addr")}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.IsNotNull(got);
      Aver.IsNotNull(got.ValidSpanUtc);
      Aver.IsNotNull(got.OrgUnit);

      Aver.AreEqual(2029, got.ValidSpanUtc.Value.End.Value.Year);
      Aver.AreEqual("addr", got.OrgUnit.Value.Address);

      Aver.IsFalse(got.HasAmorphousData);
      got.See();
    }

    [Run]
    public void Test01_map_03()
    {
      var json = new JsonDataMap()
      {
        { "ValidSpanUtc", new DateRange(DateTime.Parse("1/1/2001"), DateTime.Parse("12/31/2029"))},
        { "OrgUnit", EntityId.Parse("path@sys::addr")},
        { "Amorph", 123}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.IsNotNull(got);
      Aver.IsNotNull(got.ValidSpanUtc);
      Aver.IsNotNull(got.OrgUnit);

      Aver.AreEqual(2029, got.ValidSpanUtc.Value.End.Value.Year);
      Aver.AreEqual("addr", got.OrgUnit.Value.Address);

      Aver.IsTrue(got.HasAmorphousData);
      Aver.AreEqual(123, got.AmorphousData["Amorph"].AsInt());
      got.See();
    }

    [Run]
    public void Test01_map_04()
    {
      var json = new JsonDataMap()
      {
        { "ValidSpanUtc", null},
        { "OrgUnit", EntityId.Parse("path@sys::addr")}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.IsNotNull(got);
      Aver.IsNull(got.ValidSpanUtc);
      Aver.IsNotNull(got.OrgUnit);

      Aver.AreEqual("addr", got.OrgUnit.Value.Address);

      Aver.IsFalse(got.HasAmorphousData);
      got.See();
    }

    [Run]
    public void Test01_map_05()
    {
      var json = new JsonDataMap()
      {
        { "ValidSpanUtc", null},
        { "OrgUnit", null}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.IsNotNull(got);
      Aver.IsNull(got.ValidSpanUtc);
      Aver.IsNull(got.OrgUnit);

      Aver.IsFalse(got.HasAmorphousData);
      got.See();
    }


    [Run]
    public void TestAll_map_int()
    {
      var json = new JsonDataMap()
      {
        { "int1", 123},
        { "int2", -456}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual(123, got.Int1);
      Aver.AreEqual(-456, got.Int2);
    }

    [Run]
    public void TestAll_map_int_null()
    {
      var json = new JsonDataMap()
      {
        { "int1", null},
        { "int2", null}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual(null, got.Int1);
      Aver.AreEqual(0, got.Int2);
    }

    [Run]
    public void TestAll_map_decimal()
    {
      var json = new JsonDataMap()
      {
        { "decimal1", 123.02m},
        { "decimal2", -456.03m}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual(123.02m, got.Decimal1);
      Aver.AreEqual(-456.03m, got.Decimal2);
    }

    [Run]
    public void TestAll_map_int_decimal_null()
    {
      var json = new JsonDataMap()
      {
        { "decimal1", null},
        { "decimal2", null}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual(null, got.Decimal1);
      Aver.AreEqual(0m, got.Decimal2);
    }

    [Run]
    public void TestAll_map_datetime()
    {
      var json = new JsonDataMap()
      {
        { "datetime1", DateTime.Parse("4/5/2021")},
        { "datetime2", DateTime.Parse("7/8/2004")}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual(2021, got.DateTime1.Value.Year);
      Aver.AreEqual(2004, got.DateTime2.Year);
    }

    [Run]
    public void TestAll_map_int_datetime_null()
    {
      var json = new JsonDataMap()
      {
        { "datetime1", null},
        { "datetime2", null}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual(null, got.DateTime1);
      Aver.AreEqual(1, got.DateTime2.Year);
    }

    [Run]
    public void TestAll_map_eid()
    {
      var json = new JsonDataMap()
      {
        { "EntityId1", EntityId.Parse("a@b::c")},
        { "EntityId2", EntityId.Parse("d@e::f")}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual("c", got.EntityId1.Value.Address);
      Aver.AreEqual("f", got.EntityId2.Address);
    }

    [Run]
    public void TestAll_map_int_eid_null()
    {
      var json = new JsonDataMap()
      {
        { "EntityId1", null},
        { "EntityId2", null}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual(null, got.EntityId1);
      Aver.AreEqual(null, got.EntityId2.Address);
    }

    [Run]
    public void TestAll_map_atom()
    {
      var json = new JsonDataMap()
      {
        { "Atom1", Atom.Encode("abc")},
        { "Atom2", Atom.Encode("def")}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual("abc", got.Atom1.Value.Value);
      Aver.AreEqual("def", got.Atom2.Value);
    }

    [Run]
    public void TestAll_map_int_atom_null()
    {
      var json = new JsonDataMap()
      {
        { "Atom1", null},
        { "Atom2", null}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual(null, got.Atom1);
      Aver.AreEqual(null, got.Atom2.Value);
    }

    [Run]
    public void TestAll_map_gdid()
    {
      var json = new JsonDataMap()
      {
        { "GDID1", GDID.Parse("0:1:2")},
        { "GDID2", GDID.Parse("3:4:5")}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual(2uL, got.GDID1.Value.Counter);
      Aver.AreEqual(5uL,  got.GDID2.Counter);
    }

    [Run]
    public void TestAll_map_int_gdid_null()
    {
      var json = new JsonDataMap()
      {
        { "GDID1", null},
        { "GDID2", null}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual(null, got.GDID1);
      Aver.AreEqual(0uL, got.GDID2.Counter);
    }

    [Run]
    public void TestAll_map_rgdid()
    {
      var json = new JsonDataMap()
      {
        { "RGDID1", RGDID.Parse("378:0:1:278")},
        { "RGDID2", RGDID.Parse("379:3:4:543")}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual(378u, got.RGDID1.Value.Route);
      Aver.AreEqual(379u, got.RGDID2.Route);
      Aver.AreEqual(278uL, got.RGDID1.Value.Gdid.Counter);
      Aver.AreEqual(543uL, got.RGDID2.Gdid.Counter);
    }

    [Run]
    public void TestAll_map_int_rgdid_null()
    {
      var json = new JsonDataMap()
      {
        { "RGDID1", null},
        { "RGDID2", null}
      };

      var got = JsonReader.ToDoc<DocWithNullables>(json);
      Aver.AreEqual(null, got.RGDID1);
      Aver.AreEqual(0uL, got.RGDID2.Gdid.Counter);
    }

  }
}
