/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;

using Azos.Data;
using Azos.Scripting;
using Azos.Serialization.JSON;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class GDIDTests2
  {
    [Run("era=0 id=0")]
    [Run("era=0xaa00aa00 id=0xf000000000000001")]
    [Run("era=4294967295 id=18446744073709551615")]
    public void Test_Values(uint era, ulong id)
    {
      var g = new GDID(era, id);

      var str = g.ToString();
      var hex = g.ToHexString();

      var g2 = GDID.Parse(str);
      var g3 = GDID.Parse(hex);

      Aver.AreEqual(g, g2);
      Aver.AreEqual(g, g3);
      Aver.AreEqual(g2, g3);

      Aver.AreObjectsEqual(g, g2);
      Aver.AreObjectsEqual(g, g3);
      Aver.AreObjectsEqual(g2, g3);

      Aver.IsTrue(g == g2);
      Aver.IsTrue(g == g3);
      Aver.IsTrue(g2 == g3);

      Aver.IsFalse(g  != g3);
      Aver.IsFalse(g  != g2);
      Aver.IsFalse(g2 != g3);

      Aver.AreEqual(g.GetHashCode(), g2.GetHashCode());
      Aver.AreEqual(g.GetHashCode(), g3.GetHashCode());

      Aver.AreEqual(g.GetDistributedStableHash(), g2.GetDistributedStableHash());
      Aver.AreEqual(g.GetDistributedStableHash(), g3.GetDistributedStableHash());

      var json = new {g}.ToJson();

      "String: {0} \n Hex: {1} \n Json: {2} \n".SeeArgs(str, hex, json);


      var got = json.JsonToDataObject() as JsonDataMap;

      Aver.AreEqual(g, got["g"].AsGDID());

      var buf = g.Bytes;
      var g4 = new GDID(buf);
      Aver.AreEqual(g, g4);

      var buf2 = new byte[128];
      g.WriteIntoBuffer(buf2, 18);
      var g5 = new GDID(buf2, 18);
      Aver.AreEqual(g, g5);
    }
  }
}
