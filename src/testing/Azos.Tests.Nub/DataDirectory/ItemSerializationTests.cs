using Azos.Scripting;
using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data.Directory;
using Azos.Serialization.JSON;
using Azos.Data;

namespace Azos.Tests.Nub.DataDirectory
{
  [Runnable]
  public class ItemSerializationTests
  {
    [Run]
    public void Basic_WithIndexAndProps()
    {
      var item = new Item("A@b::address")
      {
        Data = "data string content",
        AbsoluteExpirationUtc = new DateTime(2090, 12, 25, 14, 00, 00, DateTimeKind.Utc)
      };

      item.Index = new Item.AttrMap{ {"atr1", "val1"} };
      item.Props = new Item.AttrMap { { "Content-Type", "application/bvd" }, { "wv-leader", "marx" } };

      var json = JsonWriter.Write(item);
      json.See();

      var got = new Item();
      got.ReadAsJson(json.JsonToDataObject(), false, null);

      got.See();
      Aver.AreEqual(item.Id, got.Id);

      Aver.AreEqual("A", got.Id.Type.Value);
      Aver.AreEqual("b", got.Id.System.Value);
      Aver.AreEqual("address", got.Id.Address);

      Aver.AreEqual(item.Data, got.Data);
      Aver.IsNotNull(got.Index);
      Aver.AreEqual("val1", got.Index["atr1"].AsString());

      Aver.AreEqual(2090, got.AbsoluteExpirationUtc.Value.Year);
      Aver.AreEqual(14, got.AbsoluteExpirationUtc.Value.Hour);
      Aver.IsTrue(DateTimeKind.Utc == got.AbsoluteExpirationUtc.Value.Kind);

      Aver.IsNotNull(got.Props);
      Aver.AreEqual("marx", got.Props["wv-leader"].AsString());
      Aver.IsTrue(ItemStatus.Created == got.VersionStatus);
    }

    [Run]
    public void Basic_WithoutIndexOrProps()
    {
      var item = new Item("A@b::address")
      {
        Data = "bumble beee"
      };

      item.SetVersion(new GDID(3, 7, 123456789), new DateTime(2090, 12, 25, 14, 00, 00, DateTimeKind.Utc), ItemStatus.Updated);

      var json = JsonWriter.Write(item);
      json.See();

      var got = new Item();
      got.ReadAsJson(json.JsonToDataObject(), false, null);

      got.See();
      Aver.AreEqual(item.Id, got.Id);

      Aver.AreEqual("A", got.Id.Type.Value);
      Aver.AreEqual("b", got.Id.System.Value);
      Aver.AreEqual("address", got.Id.Address);

      Aver.AreEqual(item.Data, got.Data);
      Aver.IsNull(got.Index);

      Aver.AreEqual(2090, got.LastUseUtc.Year);
      Aver.AreEqual(14, got.LastUseUtc.Hour);
      Aver.IsTrue(DateTimeKind.Utc == got.LastUseUtc.Kind);

      Aver.IsNull(got.AbsoluteExpirationUtc);

      Aver.AreEqual(item.Gdid, got.Gdid);
      Aver.IsTrue(ItemStatus.Updated == got.VersionStatus);

      Aver.IsNull(got.Props);
    }

    [Run]
    public void Basic_WithoutIndexOrProps_SkipNulls()
    {
      var item = new Item("A@b::address")
      {
        Data = "bumble beee"
      };

      item.SetVersion(new GDID(3, 7, 123456789), new DateTime(2090, 12, 25, 14, 00, 00, DateTimeKind.Utc), ItemStatus.Updated);

      var json = JsonWriter.Write(item, new JsonWritingOptions{ MapSkipNulls = true});
      json.See();

      var got = new Item();
      got.ReadAsJson(json.JsonToDataObject(), false, null);

      got.See();
      Aver.AreEqual(item.Id, got.Id);

      Aver.AreEqual("A", got.Id.Type.Value);
      Aver.AreEqual("b", got.Id.System.Value);
      Aver.AreEqual("address", got.Id.Address);

      Aver.AreEqual(item.Data, got.Data);
      Aver.IsNull(got.Index);

      Aver.AreEqual(2090, got.LastUseUtc.Year);
      Aver.AreEqual(14, got.LastUseUtc.Hour);
      Aver.IsTrue(DateTimeKind.Utc == got.LastUseUtc.Kind);

      Aver.IsNull(got.AbsoluteExpirationUtc);

      Aver.AreEqual(item.Gdid, got.Gdid);

      Aver.IsTrue(ItemStatus.Updated == got.VersionStatus);

      Aver.IsNull(got.Props);
    }
  }
}
