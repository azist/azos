/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Data;
using System.Linq;

using Azos.Data;
using Azos.Data.Adlib;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class AdlibTests
  {
    [Run]
    public void SerializeTags()
    {
      var item = new Item
      {
        Tags = new List<Tag>
        {
          new Tag(Atom.Encode("p1"), -9_777_333_000L),
          new Tag(Atom.Encode("p2"), "Hello"),
          new Tag(Atom.Encode("p3"), "Dolly"),
          new Tag(Atom.Encode("age"), 129)
        }
      };

      var json = item.ToJson();

      json.See();

      var got = JsonReader.ToDoc<Item>(json);
      Aver.IsNotNull(got);
      Aver.IsNotNull(got.Tags);
      Aver.AreEqual(4, got.Tags.Count);
      Aver.AreEqual(-9_777_333_000L, got.Tags[0].NValue);
      Aver.AreEqual("p1", got.Tags[0].Prop.Value);

      Aver.AreEqual("Hello", got.Tags[1].SValue);
      Aver.AreEqual("p2", got.Tags[1].Prop.Value);

      Aver.AreEqual("Dolly", got.Tags[2].SValue);
      Aver.AreEqual("p3", got.Tags[2].Prop.Value);

      Aver.AreEqual(129, got.Tags[3].NValue);
      Aver.AreEqual("age", got.Tags[3].Prop.Value);
    }
  }
}
