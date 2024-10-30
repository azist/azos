/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.Slim;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class Slim6Tests
  {

    [Run]
    public void Simple001()
    {
      using var ms = new MemoryStream();
      var sut = new SlimSerializer();

      var obj = new EidSimple
      {
        Eid1 = EntityId.Parse("a.b@c::addr")
      };

      sut.Serialize(ms, obj);
      ms.Seek(0, SeekOrigin.Begin);

      var got = (EidSimple)sut.Deserialize(ms);

      Aver.IsNotNull(got);
      Aver.AreNotSameRef(obj, got);
      Aver.AreEqual("a.b@c::addr", got.Eid1.AsString);
      Aver.IsNull(got.Eid2);//nullable
    }

    [Run]
    public void Simple002()
    {
      using var ms = new MemoryStream();
      var sut = new SlimSerializer();

      var obj = new EidSimple
      {
        Eid1 = EntityId.Parse("a.b@c::addr"),
        Eid2 = EntityId.Parse("a2.b2@c2::addr2"),
      };

      sut.Serialize(ms, obj);
      ms.Seek(0, SeekOrigin.Begin);

      var got = (EidSimple)sut.Deserialize(ms);

      Aver.IsNotNull(got);
      Aver.AreNotSameRef(obj, got);
      Aver.AreEqual("a.b@c::addr", got.Eid1.AsString);
      Aver.IsNotNull(got.Eid2);//nullable
      Aver.AreEqual("a2.b2@c2::addr2", got.Eid2.Value.AsString);
    }

    [Run]
    public void Simple003()
    {
      using var ms = new MemoryStream();
      var sut = new SlimSerializer();

      var obj = new EidSimple
      {
        Eid1 = EntityId.Parse("a.b@c::addr"),
        Eid2 = EntityId.Parse("a2.b2@c2::addr2"),
        Arr1 = new EntityId[] { EntityId.Parse("a3.b3@c3::addr3") , EntityId.Parse("a4.b4@c4::addr4") },
        Arr2 = new EntityId?[] { null, EntityId.Parse("a5.b5@c5::addr5") }
      };

      sut.Serialize(ms, obj);
      ms.Seek(0, SeekOrigin.Begin);

      var got = (EidSimple)sut.Deserialize(ms);

      Aver.IsNotNull(got);
      Aver.AreNotSameRef(obj, got);
      Aver.AreEqual("a.b@c::addr", got.Eid1.AsString);
      Aver.IsNotNull(got.Eid2);//nullable
      Aver.AreEqual("a2.b2@c2::addr2", got.Eid2.Value.AsString);

      Aver.IsNotNull(got.Arr1);
      Aver.AreEqual("a3.b3@c3::addr3", got.Arr1[0].AsString);
      Aver.AreEqual("a4.b4@c4::addr4", got.Arr1[1].AsString);

      Aver.IsNotNull(got.Arr2);
      Aver.IsNull(got.Arr2[0]);
      Aver.IsNotNull(got.Arr2[1]);
      Aver.AreEqual("a5.b5@c5::addr5", got.Arr2[1].Value.AsString);
    }

    [Run]
    public void Doc001()
    {
      using var ms = new MemoryStream();
      var sut = new SlimSerializer();

      var obj = new EidDoc
      {
        Eid1 = EntityId.Parse("a.b@c::addr"),
        Eid2 = EntityId.Parse("a2.b2@c2::addr2"),
        Arr1 = new EntityId[] { EntityId.Parse("a3.b3@c3::addr3"), EntityId.Parse("a4.b4@c4::addr4") },
        Arr2 = new EntityId?[] { null, EntityId.Parse("a5.b5@c5::addr5") }
      };

      sut.Serialize(ms, obj);
      ms.Seek(0, SeekOrigin.Begin);

      var got = (EidDoc)sut.Deserialize(ms);

      Aver.IsNotNull(got);
      Aver.AreNotSameRef(obj, got);
      Aver.AreEqual("a.b@c::addr", got.Eid1.AsString);
      Aver.IsNotNull(got.Eid2);//nullable
      Aver.AreEqual("a2.b2@c2::addr2", got.Eid2.Value.AsString);

      Aver.IsNotNull(got.Arr1);
      Aver.AreEqual("a3.b3@c3::addr3", got.Arr1[0].AsString);
      Aver.AreEqual("a4.b4@c4::addr4", got.Arr1[1].AsString);

      Aver.IsNotNull(got.Arr2);
      Aver.IsNull(got.Arr2[0]);
      Aver.IsNotNull(got.Arr2[1]);
      Aver.AreEqual("a5.b5@c5::addr5", got.Arr2[1].Value.AsString);
    }







    private class EidSimple
    {
      public EntityId Eid1;
      public EntityId? Eid2;
      public EntityId[] Arr1;
      public EntityId?[] Arr2;
    }

    private class EidDoc : TypedDoc
    {
      public EntityId Eid1 { get; set; }
      public EntityId? Eid2 { get; set; }
      public EntityId[] Arr1 { get; set; }
      public EntityId?[] Arr2 { get; set; }
    }

  }
}
