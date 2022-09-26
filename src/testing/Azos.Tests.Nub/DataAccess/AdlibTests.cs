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
    public void SerializeTags_1()
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

    [Run]
    public void SerializeTags_2()
    {
      var item = new Item
      {
        Tags = new List<Tag>
        {
          new Tag(Atom.Encode("p1"), 123),
          new Tag(Atom.Encode("p2"), "Hello"),
          new Tag(Atom.Encode("p3"), "Dolly")
        }
      };

      var json = item.ToJson();

      json.See();

      var got = JsonReader.ToDoc<Item>(json);
      Aver.IsNotNull(got);
      Aver.IsNotNull(got.Tags);
      Aver.AreEqual(3, got.Tags.Count);
      Aver.AreEqual(123, got.Tags[0].NValue);
      Aver.AreEqual("p1", got.Tags[0].Prop.Value);

      Aver.AreEqual("Hello", got.Tags[1].SValue);
      Aver.AreEqual("p2", got.Tags[1].Prop.Value);

      Aver.AreEqual("Dolly", got.Tags[2].SValue);
      Aver.AreEqual("p3", got.Tags[2].Prop.Value);

    }

    [Run]
    public void SerializeTags_3()
    {
      var item = new Item
      {
        Tags = new List<Tag>
        {
          new Tag(Atom.Encode("p1"), 123),
          new Tag(Atom.Encode("p2"), "Hello"),
          new Tag(Atom.Encode("p3"), "Dolly")
        }
      };

      var json = item.ToJson();

      json.See();

      var map = JsonReader.DeserializeDataObject(json) as JsonDataMap;

      var got = JsonReader.ToDoc(typeof(Item), map) as Item;
      Aver.IsNotNull(got);
      Aver.IsNotNull(got.Tags);
      Aver.AreEqual(3, got.Tags.Count);
      Aver.AreEqual(123, got.Tags[0].NValue);
      Aver.AreEqual("p1", got.Tags[0].Prop.Value);

      Aver.AreEqual("Hello", got.Tags[1].SValue);
      Aver.AreEqual("p2", got.Tags[1].Prop.Value);

      Aver.AreEqual("Dolly", got.Tags[2].SValue);
      Aver.AreEqual("p3", got.Tags[2].Prop.Value);

    }

    [Run]
    public void SerializeTags_4()
    {
      var requestJson = @"{
  'item': {
        'gdid': '0:2:305',
        'space': 'g8formf',
        'collection': 'dima1',
        'segment': 100,
        'shardtopic': null,
        'createutc': 0,
        'origin': 'a',
        'headers': '',
        'contenttype': 'bin',
        'content': 'AP8BAABHKuRAAAAUAP8BAABHKuRAAAAU',
        'Tags': [{'p': 'p1', 'n': -1},{'p': 'p2', 's': 'Hello'},{'p': 'p3', 's': 'Dolly'}]
  }
}";

      var requestMap = JsonReader.DeserializeDataObject(requestJson) as JsonDataMap;
      var map = requestMap["item"] as JsonDataMap;

      var got = JsonReader.ToDoc(typeof(Item), map) as Item;
      Aver.IsNotNull(got);
      Aver.IsNotNull(got.Tags);
      Aver.AreEqual(3, got.Tags.Count);
      Aver.AreEqual(-1, got.Tags[0].NValue);
      Aver.AreEqual("p1", got.Tags[0].Prop.Value);

      Aver.AreEqual("Hello", got.Tags[1].SValue);
      Aver.AreEqual("p2", got.Tags[1].Prop.Value);

      Aver.AreEqual("Dolly", got.Tags[2].SValue);
      Aver.AreEqual("p3", got.Tags[2].Prop.Value);

    }

  }
}
