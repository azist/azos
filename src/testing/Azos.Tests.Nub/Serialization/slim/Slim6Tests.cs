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
      public EntityId? Eid { get; set; }
      public EntityId[] Arr1 { get; set; }
      public EntityId?[] Arr2 { get; set; }
    }

  }
}
