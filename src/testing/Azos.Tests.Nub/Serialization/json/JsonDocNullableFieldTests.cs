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
  [Runnable]
  public class JsonDocNullableFieldTests
  {
    public class DocWithNullables : AmorphousTypedDoc
    {
      public override bool AmorphousDataEnabled => true;

      [Field]
      public DateRange? ValidSpanUtc { get; set; }

      [Field]
      public EntityId? OrgUnit { get; set; }
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

  }
}
