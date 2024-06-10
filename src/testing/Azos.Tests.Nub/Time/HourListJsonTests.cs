/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Time;
using Azos.Scripting;
using Azos.Serialization.JSON;
using System.Linq;

namespace Azos.Tests.Nub.Time
{
  [Runnable]
  public class HourListJsonTests
  {
    [Run]
    public void Unassigned()
    {
      var x = new HourList();
      Aver.IsFalse(x.IsAssigned);
    }

    [Run("spec='1am-2am'")]
    [Run("spec='1am-2:15am, 8am-12pm'")]
    [Run("spec='1am-2:15am, 8am-12pm, 12pm-3:15pm, 3:45pm-12:00am'")]
    public void Roundtrip1(string spec)
    {
      var x = new HourList(spec);

      var json1 = x.ToJson();
      var json2 = x.ToJson(new JsonWritingOptions{  Purpose = JsonSerializationPurpose.Marshalling});

      "JSON1: {0}\n JSON2: {1}\n".SeeArgs(json1, json2);

      var got1 = JsonReader.Deserialize(json1);
      var got2 = JsonReader.Deserialize(json2);

      Aver.IsTrue(got1 is string);
      Aver.IsTrue(got2 is JsonDataMap);

      var y1 = new HourList().ReadAsJson(got1, false, null);
      var y2 = new HourList().ReadAsJson(got1, false, null);

      Aver.IsTrue(y1.match);
      Aver.IsTrue(y2.match);

      Aver.IsTrue(x.Equals(y1.self));
      Aver.IsTrue(x.Equals(y2.self));

      "Deserialized1: {0}\n Deserialized2: {1}\n".SeeArgs(y1, y2);
    }

  }
}
