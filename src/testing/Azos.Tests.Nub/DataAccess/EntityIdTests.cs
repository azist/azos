/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class EntityIdTests
  {
    [Run]
    public void NotAssigned()
    {
      var v = default(EntityId);
      Aver.IsFalse(v.IsAssigned);
      Aver.IsFalse(v.CheckRequired(null));
    }

    [Run]
    public void NotAssigned_GetHashCodeToStringEquals()
    {
      var v = default(EntityId);
      Aver.AreEqual(0, v.GetHashCode());
      Aver.AreEqual(0ul, v.GetDistributedStableHash());
      Aver.AreEqual("", v.ToString());
      Aver.AreEqual("", v.AsString);
      Aver.IsTrue(default(EntityId).Equals(v));
      Aver.IsTrue(default(EntityId) == v);
    }

    [Run]
    public void HashCodeEquals()
    {
      var v1 = new EntityId(Atom.Encode("sys"), Atom.Encode("tp1"), Atom.ZERO, "address");
      var v2 = new EntityId(Atom.Encode("sys"), Atom.Encode("tp2"), Atom.ZERO, "address");
      var v3 = new EntityId(Atom.Encode("sys"), Atom.Encode("tp1"), Atom.ZERO, "address2");
      var v4 = new EntityId(Atom.Encode("sYs"), Atom.Encode("tp1"), Atom.ZERO, "address");
      var v5 = new EntityId(Atom.Encode("sys"), Atom.Encode("tp1"), Atom.ZERO, "address");

      var v6 = new EntityId(Atom.Encode("sys"), Atom.ZERO, Atom.ZERO, "address");
      var v7 = new EntityId(Atom.Encode("sys"), Atom.ZERO, Atom.ZERO, "address");
      var v8 = new EntityId(Atom.Encode("sys"), Atom.ZERO, Atom.ZERO, "address-1");


      var v9 = new EntityId(Atom.Encode("sys"), Atom.Encode("tp1"), Atom.Encode("sch1"), "address-1");
      var v10 = new EntityId(Atom.Encode("sys"), Atom.Encode("tp1"), Atom.Encode("sch1"), "address-1");
      var v11 = new EntityId(Atom.Encode("sys"), Atom.Encode("tp1"), Atom.Encode("sch2"), "address-1");


      Aver.AreObjectsEqual(v1, v5);
      Aver.AreObjectsEqual(v5, v1);
      Aver.AreObjectsNotEqual(v1, v2);
      Aver.AreObjectsNotEqual(v1, v3);
      Aver.AreObjectsNotEqual(v1, v4);
      Aver.AreObjectsNotEqual(v4, v5);

      Aver.AreObjectsEqual(v6, v7);
      Aver.AreObjectsNotEqual(v7, v8);


      Aver.AreEqual(v1.GetHashCode(), v5.GetHashCode());
      Aver.AreEqual(v5.GetHashCode(), v1.GetHashCode());
      Aver.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreNotEqual(v1.GetHashCode(), v3.GetHashCode());
      Aver.AreNotEqual(v1.GetHashCode(), v4.GetHashCode());
      Aver.AreNotEqual(v4.GetHashCode(), v5.GetHashCode());
      Aver.AreEqual(v6.GetHashCode(), v7.GetHashCode());
      Aver.AreNotEqual(v7.GetHashCode(), v8.GetHashCode());

      Aver.AreEqual(v1.GetDistributedStableHash(), v5.GetDistributedStableHash());
      Aver.AreEqual(v5.GetDistributedStableHash(), v1.GetDistributedStableHash());
      Aver.AreNotEqual(v1.GetDistributedStableHash(), v2.GetDistributedStableHash());
      Aver.AreNotEqual(v1.GetDistributedStableHash(), v3.GetDistributedStableHash());
      Aver.AreNotEqual(v1.GetDistributedStableHash(), v4.GetDistributedStableHash());
      Aver.AreNotEqual(v4.GetDistributedStableHash(), v5.GetDistributedStableHash());
      Aver.AreEqual(v6.GetDistributedStableHash(), v7.GetDistributedStableHash());
      Aver.AreNotEqual(v7.GetDistributedStableHash(), v8.GetDistributedStableHash());

      Aver.AreObjectsEqual(v9, v10);
      Aver.AreObjectsNotEqual(v9, v11);

      Aver.IsTrue(v9 == v10);
      Aver.IsFalse(v9 != v10);

      Aver.IsFalse(v9 == v11);
      Aver.IsTrue(v9 != v11);

      Aver.AreObjectsEqual(v9, v10);
      Aver.AreObjectsNotEqual(v9, v11);

      Aver.AreEqual(v9.GetHashCode(), v10.GetHashCode());
      Aver.AreNotEqual(v9.GetHashCode(), v11.GetHashCode());

      Aver.AreEqual(v9.GetDistributedStableHash(), v10.GetDistributedStableHash());
      Aver.AreNotEqual(v9.GetDistributedStableHash(), v11.GetDistributedStableHash());

    }

    [Run]
    public void TryParse00()
    {
      Aver.IsTrue(EntityId.TryParse(null, out var v));
      Aver.IsFalse(v.IsAssigned);

      Aver.IsTrue(EntityId.TryParse("", out v));
      Aver.IsFalse(v.IsAssigned);

      Aver.IsTrue(EntityId.TryParse("               ", out v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse01()
    {
      Aver.IsTrue(EntityId.TryParse("a@b::adr1", out var v));
      Aver.IsTrue(v.IsAssigned);
      Aver.AreEqual(Atom.Encode("a"), v.Type);
      Aver.AreEqual(Atom.Encode("b"), v.System);
      Aver.AreEqual("adr1", v.Address);
    }

    [Run]
    public void TryParse02()
    {
      Aver.IsTrue(EntityId.TryParse("b::adr1", out var v));
      Aver.IsTrue(v.IsAssigned);
      Aver.AreEqual(Atom.ZERO, v.Type);
      Aver.AreEqual(Atom.Encode("b"), v.System);
      Aver.AreEqual("adr1", v.Address);
    }

    [Run]
    public void TryParse03()
    {
      Aver.IsTrue(EntityId.TryParse("system01::@://long-address::-string", out var v));
      Aver.IsTrue(v.IsAssigned);
      Aver.AreEqual(Atom.ZERO, v.Type);
      Aver.AreEqual(Atom.Encode("system01"), v.System);
      Aver.AreEqual("@://long-address::-string", v.Address);
    }

    [Run]
    public void TryParse04()
    {
      Aver.IsFalse(EntityId.TryParse("::abc", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse05()
    {
      Aver.IsFalse(EntityId.TryParse("aa::", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse06()
    {
      Aver.IsFalse(EntityId.TryParse("bbb@aa::", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse07()
    {
      Aver.IsFalse(EntityId.TryParse("bbb@::", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse08()
    {
      Aver.IsFalse(EntityId.TryParse("aaa::             ", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse09()
    {
      Aver.IsFalse(EntityId.TryParse("         @aaa::gggg", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse10()
    {
      Aver.IsFalse(EntityId.TryParse("@", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse11()
    {
      Aver.IsFalse(EntityId.TryParse("a b@dd::aaa", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse12()
    {
      Aver.IsFalse(EntityId.TryParse("ab@d d::aaa", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse13()
    {
      Aver.IsFalse(EntityId.TryParse("ab@d*d::aaa", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse14()
    {
      Aver.IsFalse(EntityId.TryParse("ab@dd::                             ", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse15()
    {
      Aver.IsFalse(EntityId.TryParse("::", out var v));
      Aver.IsFalse(v.IsAssigned);
    }


    [Run]
    public void TryParse16()
    {
      Aver.IsTrue(EntityId.TryParse("vendor.gdid@ecom::1234", out var v));
      Aver.IsTrue(v.IsAssigned);

      Aver.AreEqual("vendor", v.Type.Value);
      Aver.AreEqual("gdid", v.Schema.Value);
      Aver.AreEqual("ecom", v.System.Value);
      Aver.AreEqual("1234", v.Address);
    }

    [Run]
    public void TryParse17()
    {
      Aver.IsTrue(EntityId.TryParse("vendor.@ecom::1234", out var v));
      Aver.IsTrue(v.IsAssigned);

      Aver.AreEqual("vendor", v.Type.Value);
      Aver.IsTrue(v.Schema.IsZero);
      Aver.AreEqual("ecom", v.System.Value);
      Aver.AreEqual("1234", v.Address);
    }

    [Run]
    public void TryParse17_1()
    {
      Aver.IsFalse(EntityId.TryParse("vendor. @ecom::1234", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse18()
    {
      Aver.IsFalse(EntityId.TryParse("vendor.gdiddddddddddddddddddddddddddddd@ecom::1234", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse19()
    {
      Aver.IsFalse(EntityId.TryParse(".@ecom::1234", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse20()
    {
      Aver.IsFalse(EntityId.TryParse(" . @ecom::1234", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse21()
    {
      Aver.IsFalse(EntityId.TryParse(" . . @ecom::1234", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse22()
    {
      Aver.IsFalse(EntityId.TryParse(".gdid@ecom::1234", out var v));
      Aver.IsFalse(v.IsAssigned);
    }

    [Run]
    public void TryParse23()
    {
      Aver.IsFalse(EntityId.TryParse(" .gdid@ecom::1234", out var v));
      Aver.IsFalse(v.IsAssigned);
    }


    [Run]
    public void JSON01()
    {
      var v = EntityId.Parse("abc@def::12:15:178");
      var obj = new { a = v };
      var json = obj.ToJson();
      json.See();
      var map = json.JsonToDataObject() as JsonDataMap;
      var got = EntityId.Parse(map["a"].ToString());

      Aver.AreEqual(v, got);
    }

    public class Doc1 : TypedDoc
    {
      [Field] public EntityId V1 { get; set; }
      [Field] public EntityId? V2 { get; set; }
    }

    [Run]
    public void JSON02()
    {
      var d1 = new Doc1 { V1 = EntityId.Parse("abc@def::12:15:178") };
      var json = d1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);
      json.See();
      var got = JsonReader.ToDoc<Doc1>(json);
      got.See();

      Aver.AreEqual(d1.V1, got.V1);
      Aver.IsNull(got.V2);
    }

    [Run]
    public void JSON03()
    {
      var d1 = new Doc1 { V1 = EntityId.Parse("abc@def::12:15:178"), V2 = EntityId.Parse("lic::i9973od") };
      var json = d1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);
      json.See();
      var got = JsonReader.ToDoc<Doc1>(json);
      got.See();

      Aver.AreEqual(d1.V1, got.V1);
      Aver.AreEqual(d1.V2, got.V2);
    }

    [Run]
    public void JSON04()
    {
      var d1 = new Doc1 { V1 = EntityId.Parse("abc@def::abc@def::456"), V2 = EntityId.Parse("lic:::::") };
      var json = d1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);
      json.See();
      var got = JsonReader.ToDoc<Doc1>(json);
      got.See();

      Aver.AreEqual("abc@def::456", got.V1.Address);
      Aver.AreEqual(":::", got.V2.Value.Address);
    }

    [Run]
    public void JSON05()
    {
      var d1 = new Doc1 { V1 = EntityId.Parse("abc.int@sys1::address1"), V2 = EntityId.Parse("sys2::address2") };
      var json = d1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);
      json.See();
      var got = JsonReader.ToDoc<Doc1>(json);
      got.See();

      Aver.AreEqual("sys1", got.V1.System.Value);
      Aver.AreEqual("int", got.V1.Schema.Value);
      Aver.AreEqual("address1", got.V1.Address);

      Aver.AreEqual("sys2", got.V2.Value.System.Value);
      Aver.AreEqual("address2", got.V2.Value.Address);
    }

    [Run]
    public void JSON06()
    {
      var d1 = new Doc1 { V1 = EntityId.Parse("abc.int@sys1::address1"), V2 = null };
      var json = d1.ToJson(JsonWritingOptions.PrettyPrintRowsAsMap);
      json.See();
      var got = JsonReader.ToDoc<Doc1>(json);
      got.See();

      Aver.AreEqual("sys1", got.V1.System.Value);
      Aver.AreEqual("int", got.V1.Schema.Value);
      Aver.AreEqual("address1", got.V1.Address);

      Aver.IsTrue( got.V2 == null);
    }


    [Run]
    public void Composite01()
    {
      var id1 = new EntityId(Atom.Encode("sys1"), Atom.Encode("t1"), new { z = 1, a = 2 });
      var id2 = new EntityId(Atom.Encode("sys1"), Atom.Encode("t1"), new { a = 2, z = 1 });

      Aver.IsTrue(id1 == id2);
      Aver.IsTrue(id1.IsCompositeAddress);
      Aver.IsTrue(id2.IsCompositeAddress);

      var map = id1.CompositeAddress;

      Aver.AreEqual(1, map["z"].AsInt());
      Aver.AreEqual(2, map["a"].AsInt());
    }
  }
}
