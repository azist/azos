/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub.DataAccess
{
  [Runnable]
  public class ShardingTests
  {
    [Run]
    public void String_001()
    {
      Aver.AreEqual(0ul, ShardingUtils.StringToShardingID(null));
    }

    [Run]
    public void String_002()
    {
      Aver.AreEqual(0ul, ShardingUtils.StringToShardingID(""));
    }

    [Run]
    public void String_003()
    {
      Aver.AreEqual(ShardingUtils.StringToShardingID("a"), ShardingUtils.StringToShardingID("a"));
    }

    [Run]
    public void String_004()
    {
      Aver.AreEqual(ShardingUtils.StringToShardingID("abc"), ShardingUtils.StringToShardingID("abc"));
    }

    [Run]
    public void String_005()
    {
      Aver.AreEqual(ShardingUtils.StringToShardingID("abcD"), ShardingUtils.StringToShardingID("abcD"));
      Aver.AreNotEqual(ShardingUtils.StringToShardingID("abcD"), ShardingUtils.StringToShardingID("abcd"));
    }

    [Run]
    public void String_006()
    {
      Aver.AreEqual(ShardingUtils.StringToShardingID("abcDefghuiop"), ShardingUtils.StringToShardingID("abcDefghuiop"));
    }

    [Run]
    public void String_007()
    {
      Aver.AreEqual(ShardingUtils.StringToShardingID("久"), ShardingUtils.StringToShardingID("久"));
      Aver.AreEqual(ShardingUtils.StringToShardingID("久有归天愿"), ShardingUtils.StringToShardingID("久有归天愿"));
      Aver.AreEqual(ShardingUtils.StringToShardingID("久有归天愿八十三年过去到处群魔乱舞"), ShardingUtils.StringToShardingID("久有归天愿八十三年过去到处群魔乱舞"));
    }

    [Run]
    public void String_008()
    {
      Aver.AreEqual(ShardingUtils.StringToShardingID("водах"), ShardingUtils.StringToShardingID("водах"));
      Aver.AreEqual(ShardingUtils.StringToShardingID("Американские суда находятся в международных водах"), ShardingUtils.StringToShardingID("Американские суда находятся в международных водах"));
    }

    [Run("v=a"), Run("v=ab"), Run("v=ba"),
     Run("v=0123456789"), Run("v=9123456780"),
     Run("v=0123456789aaa"), Run("v=9123456780aaa"),
     Run("v=久"), Run("v=久有"), Run("v=有久"), Run("v=' 久'"), Run("v='久 '")
    ]
    public void String_009(string v)
    {
      var h = ShardingUtils.StringToShardingID(v);
      "Hash: {0:x2}".SeeArgs(h);
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(v), h);
    }

    [Run]
    public void GDID_001()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(new GDID(0,0,0)), ShardingUtils.ObjectToShardingID(GDID.ZERO));
    }

    [Run]
    public void GDID_002()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(new GDID(1, 0, 0)), ShardingUtils.ObjectToShardingID(new GDID(1, 0, 0)));
    }

    [Run]
    public void GDID_003()
    {
      Aver.AreNotEqual(ShardingUtils.ObjectToShardingID(new GDID(1, 1, 0)), ShardingUtils.ObjectToShardingID(new GDID(1, 0, 0)));
    }

    [Run]
    public void GDID_004()
    {
      Aver.AreNotEqual(ShardingUtils.ObjectToShardingID(new GDID(1, 1, 2)), ShardingUtils.ObjectToShardingID(new GDID(1, 1, 0)));
    }

    [Run]
    public void GDID_005()
    {
      Aver.AreNotEqual(ShardingUtils.ObjectToShardingID(new GDID(1, 1, 123)), ShardingUtils.ObjectToShardingID(new GDID(2, 1, 123)));
    }

    [Run]
    public void GDID_006()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(new GDID(2, 1, 123)), ShardingUtils.ObjectToShardingID(new GDID(2, 1, 123)));
    }


    [Run]
    public void GUID_001()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(Guid.Empty), ShardingUtils.ObjectToShardingID(Guid.Empty));
    }

    [Run]
    public void GUID_002()
    {
      var guid = Guid.NewGuid();
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(guid), ShardingUtils.ObjectToShardingID(guid));
    }

    [Run]
    public void GUID_003()
    {
      var guid1 = Guid.NewGuid();
      var guid2 = Guid.NewGuid();
      Aver.AreNotEqual(ShardingUtils.ObjectToShardingID(guid1), ShardingUtils.ObjectToShardingID(guid2));
    }


    [Run]
    public void INT_001()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(0), ShardingUtils.ObjectToShardingID(0));
    }

    [Run]
    public void INT_002()
    {
      Aver.AreNotEqual(ShardingUtils.ObjectToShardingID(-1), ShardingUtils.ObjectToShardingID(0));
    }

    [Run]
    public void INT_003()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(1), ShardingUtils.ObjectToShardingID(1));
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(1234), ShardingUtils.ObjectToShardingID(1234));
    }

    [Run]
    public void INT_004()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(int.MinValue), ShardingUtils.ObjectToShardingID(int.MinValue));
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(int.MaxValue), ShardingUtils.ObjectToShardingID(int.MaxValue));
    }

    [Run]
    public void uINT_001()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(0u), ShardingUtils.ObjectToShardingID(0u));
    }

    [Run]
    public void uINT_002()
    {
      Aver.AreNotEqual(ShardingUtils.ObjectToShardingID(1u), ShardingUtils.ObjectToShardingID(0u));
    }

    [Run]
    public void uINT_003()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(1u), ShardingUtils.ObjectToShardingID(1u));
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(1234u), ShardingUtils.ObjectToShardingID(1234u));
    }

    [Run]
    public void uINT_004()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(uint.MinValue), ShardingUtils.ObjectToShardingID(uint.MinValue));
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(uint.MaxValue), ShardingUtils.ObjectToShardingID(uint.MaxValue));
    }

    [Run]
    public void LONG_001()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(0L), ShardingUtils.ObjectToShardingID(0L));
    }

    [Run]
    public void LONG_002()
    {
      Aver.AreNotEqual(ShardingUtils.ObjectToShardingID(1L), ShardingUtils.ObjectToShardingID(0L));
    }

    [Run]
    public void LONG_003()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(1L), ShardingUtils.ObjectToShardingID(1L));
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(1234L), ShardingUtils.ObjectToShardingID(1234L));
    }

    [Run]
    public void LONG_004()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(long.MinValue), ShardingUtils.ObjectToShardingID(long.MinValue));
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(long.MaxValue), ShardingUtils.ObjectToShardingID(long.MaxValue));
    }

    [Run]
    public void uLONG_001()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(0uL), ShardingUtils.ObjectToShardingID(0uL));
    }

    [Run]
    public void uLONG_002()
    {
      Aver.AreNotEqual(ShardingUtils.ObjectToShardingID(1uL), ShardingUtils.ObjectToShardingID(0uL));
    }

    [Run]
    public void uLONG_003()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(1uL), ShardingUtils.ObjectToShardingID(1uL));
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(1234uL), ShardingUtils.ObjectToShardingID(1234uL));
    }

    [Run]
    public void uLONG_004()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(ulong.MinValue), ShardingUtils.ObjectToShardingID(ulong.MinValue));
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(ulong.MaxValue), ShardingUtils.ObjectToShardingID(ulong.MaxValue));
    }

    [Run]
    public void BOOL()
    {
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(true), ShardingUtils.ObjectToShardingID(true));
      Aver.AreEqual(ShardingUtils.ObjectToShardingID(false), ShardingUtils.ObjectToShardingID(false));
      Aver.AreNotEqual(ShardingUtils.ObjectToShardingID(true), ShardingUtils.ObjectToShardingID(false));
    }

  }
}
