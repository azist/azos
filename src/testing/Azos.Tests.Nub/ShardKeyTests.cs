/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class ShardKeyTests
  {
    [Run]
    public void String_001()
    {
      Aver.AreEqual(0ul, ShardKey.ForString(null));
    }

    [Run]
    public void String_002()
    {
      Aver.AreEqual(0ul, ShardKey.ForString(""));
    }

    [Run]
    public void String_003()
    {
      Aver.AreEqual(ShardKey.ForString("a"), ShardKey.ForString("a"));
    }

    [Run]
    public void String_004()
    {
      Aver.AreEqual(ShardKey.ForString("abc"), ShardKey.ForString("abc"));
    }

    [Run]
    public void String_005()
    {
      Aver.AreEqual(ShardKey.ForString("abcD"), ShardKey.ForString("abcD"));
      Aver.AreNotEqual(ShardKey.ForString("abcD"), ShardKey.ForString("abcd"));
    }

    [Run]
    public void String_006()
    {
      Aver.AreEqual(ShardKey.ForString("abcDefghuiop"), ShardKey.ForString("abcDefghuiop"));
    }

    [Run]
    public void String_007()
    {
      Aver.AreEqual(ShardKey.ForString("久"), ShardKey.ForString("久"));
      Aver.AreEqual(ShardKey.ForString("久有归天愿"), ShardKey.ForString("久有归天愿"));
      Aver.AreEqual(ShardKey.ForString("久有归天愿八十三年过去到处群魔乱舞"), ShardKey.ForString("久有归天愿八十三年过去到处群魔乱舞"));
    }

    [Run]
    public void String_008()
    {
      Aver.AreEqual(ShardKey.ForString("водах"), ShardKey.ForString("водах"));
      Aver.AreEqual(ShardKey.ForString("Американские суда находятся в международных водах"), ShardKey.ForString("Американские суда находятся в международных водах"));
    }

    [Run("v=a"), Run("v=b"), Run("v=A"),
     Run("v=ab"), Run("v=ba"),
     Run("v=0123456789"), Run("v=9123456780"),
     Run("v=0123456789aaa"), Run("v=9123456780aaa"),
     Run("v=久"), Run("v=久有"), Run("v=有久"), Run("v=' 久'"), Run("v='久 '"),
     Run("v=sysCLE001"), Run("v=sysCLE002"),
     Run("v=sysCHI001"), Run("v=sysCHI002")
    ]
    public void String_009(string v)
    {
      var h = ShardKey.ForString(v);
      "Hash: {0:x2}".SeeArgs(h);
      Aver.AreEqual(new ShardKey(v).Hash, h);
    }


    [Run("v='0:0:0'"),
     Run("v='0:0:1'"), Run("v='0:0:2'"),
     Run("v='0:1:0'"), Run("v='0:2:0'"),
     Run("v='1:0:0'"), Run("v='2:0:0'"),
     Run("v='0:3:1'"), Run("v='0:3:2'"),
     Run("v='1:3:1'"), Run("v='1:3:2'"),
     Run("v='1:5:4321'"), Run("v='0:1:99312'")
    ]
    public void GDID_000(GDID v)
    {
      var h = new ShardKey(v);
      "GDID Hash: {0:x2}".SeeArgs(h.Hash);
      Aver.AreEqual(new ShardKey(v).Hash, h.Hash);
      Aver.AreEqual(new ShardKey(v), h);
      Aver.IsTrue(new ShardKey(v).Equals(h));
      Aver.IsTrue(new ShardKey(v) ==  h);
      Aver.IsFalse(new ShardKey(v) != h);
      Aver.IsTrue(new ShardKey(v).GetHashCode() == h.GetHashCode());
    }


    [Run]
    public void GDID_001()
    {
      Aver.AreEqual(new ShardKey(new GDID(0,0,0)), new ShardKey(GDID.ZERO));
    }

    [Run]
    public void GDID_002()
    {
      Aver.AreEqual(new ShardKey(new GDID(1, 0, 0)), new ShardKey(new GDID(1, 0, 0)));
    }

    [Run]
    public void GDID_003()
    {
      Aver.AreNotEqual(new ShardKey(new GDID(1, 1, 0)), new ShardKey(new GDID(1, 0, 0)));
    }

    [Run]
    public void GDID_004()
    {
      Aver.AreNotEqual(new ShardKey(new GDID(1, 1, 2)), new ShardKey(new GDID(1, 1, 0)));
    }

    [Run]
    public void GDID_005()
    {
      Aver.AreNotEqual(new ShardKey(new GDID(1, 1, 123)), new ShardKey(new GDID(2, 1, 123)));
    }

    [Run]
    public void GDID_006()
    {
      Aver.AreEqual(new ShardKey(new GDID(2, 1, 123)), new ShardKey(new GDID(2, 1, 123)));
    }


    [Run]
    public void GUID_001()
    {
      Aver.AreEqual(new ShardKey(Guid.Empty).Hash, new ShardKey(Guid.Empty).Hash);
    }

    [Run]
    public void GUID_002()
    {
      var guid = Guid.NewGuid();
      Aver.AreEqual(new ShardKey(guid).Hash, new ShardKey(guid).Hash);
    }

    [Run]
    public void GUID_003()
    {
      var guid1 = Guid.NewGuid();
      var guid2 = Guid.NewGuid();
      Aver.AreNotEqual(new ShardKey(guid1).Hash, new ShardKey(guid2).Hash);
    }


    [Run]
    public void uINT_001()
    {
      Aver.AreEqual(new ShardKey(0u), new ShardKey(0u));
    }

    [Run]
    public void uINT_002()
    {
      Aver.AreNotEqual(new ShardKey(1u), new ShardKey(0u));
    }

    [Run]
    public void uINT_003()
    {
      Aver.AreEqual(new ShardKey(1u), new ShardKey(1u));
      Aver.AreEqual(new ShardKey(1234u), new ShardKey(1234u));
    }

    [Run]
    public void uINT_004()
    {
      Aver.AreEqual(new ShardKey(uint.MinValue), new ShardKey(uint.MinValue));
      Aver.AreEqual(new ShardKey(uint.MaxValue), new ShardKey(uint.MaxValue));
    }

    [Run]
    public void uLONG_001()
    {
      Aver.AreEqual(new ShardKey(0uL), new ShardKey(0uL));
    }

    [Run]
    public void uLONG_002()
    {
      Aver.AreNotEqual(new ShardKey(1uL), new ShardKey(0uL));
    }

    [Run]
    public void uLONG_003()
    {
      Aver.AreEqual(new ShardKey(1uL), new ShardKey(1uL));
      Aver.AreEqual(new ShardKey(1234uL), new ShardKey(1234uL));
    }

    [Run]
    public void uLONG_004()
    {
      Aver.AreEqual(new ShardKey(ulong.MinValue), new ShardKey(ulong.MinValue));
      Aver.AreEqual(new ShardKey(ulong.MaxValue), new ShardKey(ulong.MaxValue));
    }


    //todo: Use the site to check specific hashes for ulongs, strings, byte[] and dates
    //https://md5calc.com/hash/fnv1a64


  }
}
