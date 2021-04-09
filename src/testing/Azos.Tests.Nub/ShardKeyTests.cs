/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Data.Idgen;
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

    [Run] public void FNV1A64_String0a() => Aver.AreEqual(0ul, ShardKey.ForString(null));
    [Run] public void FNV1A64_String0b() => Aver.AreEqual(0ul, ShardKey.ForString(""));
    [Run] public void FNV1A64_String1() => Aver.AreEqual(0xaf63dc4c8601ec8cUL, ShardKey.ForString("a"));
    [Run] public void FNV1A64_String2() => Aver.AreEqual(0x089c4407b545986aUL, ShardKey.ForString("ab"));
    [Run] public void FNV1A64_String3() => Aver.AreEqual(0xe71fe3190541c5beUL, ShardKey.ForString("ab "));
    [Run] public void FNV1A64_String4() => Aver.AreEqual(0xc2d3aa17cdf6a148UL, ShardKey.ForString(" ab"));
    [Run] public void FNV1A64_String5() => Aver.AreEqual(0xd8bff0beeb476aabUL, ShardKey.ForString("Capital"));
    [Run] public void FNV1A64_String6() => Aver.AreEqual(0xb33b26031858e24bUL, ShardKey.ForString("capital"));
    [Run] public void FNV1A64_String7() => Aver.AreEqual(0x181ff1a88c7c1e81UL, ShardKey.ForString("very very long string which is much longer than other strings IN this set"));
    [Run] public void FNV1A64_String8() => Aver.AreEqual(0x37458A3018A18365UL, ShardKey.ForString("久有归天愿"));

    [Run] public void FNV1A64_Bytes0a() => Aver.AreEqual(0ul, ShardKey.ForBytes(null));
    [Run] public void FNV1A64_Bytes0b() => Aver.AreEqual(0ul, ShardKey.ForBytes(new byte[0]));
    [Run] public void FNV1A64_Bytes1() => Aver.AreEqual(0xaf63dc4c8601ec8cUL, ShardKey.ForBytes(new byte[]{ (byte)'a' }));
    [Run] public void FNV1A64_Bytes2() => Aver.AreEqual(0x089c4407b545986aUL, ShardKey.ForBytes(new byte[]{ (byte)'a', (byte)'b' }));

    [Run] public void FNV1A64_Ulong0() => Aver.AreEqual(0ul, ShardKey.ForUlong(0));
    [Run] public void FNV1A64_Ulong1() => Aver.AreEqual(0x593c2a4dd0080bfdUL, ShardKey.ForUlong(0x3736353433323130));//01234567 string
    [Run] public void FNV1A64_Ulong2() => Aver.AreEqual(0xb2a5d3eebe6504b5UL, ShardKey.ForUlong(0x3031323334353637));//76543210 string


    [Run]
    public void ShardKey_Gdid()
    {
      var v1 = new ShardKey(new GDID(0, 0, 0));
      var v2 = new ShardKey(new GDID(0, 0, 0));
      Aver.IsTrue(ShardKey.Type.Gdid == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueGdid, v2.ValueGdid);
      Aver.IsTrue(v1 == v2);

      "Value: {0}".SeeArgs(v1);//ToString()

      v1 = new ShardKey(new GDID(0, 0, 123));
      v2 = new ShardKey(new GDID(0, 0, 123));
      Aver.IsTrue(ShardKey.Type.Gdid == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueGdid, v2.ValueGdid);
      Aver.IsTrue(v1 == v2);
      Aver.AreEqual(new GDID(0, 0, 123), v1.ValueGdid);

      "Value: {0}".SeeArgs(v1);//ToString()

      v2 = new ShardKey(new GDID(0, 0, 124));
      Aver.AreNotEqual(v1, v2);
      Aver.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreNotEqual(v1.Hash, v2.Hash);
      Aver.AreNotEqual(v1.ValueGdid, v2.ValueGdid);
      Aver.IsTrue(v1 != v2);
    }

    [Run]
    public void ShardKey_Atom()
    {
      var v1 = new ShardKey(new Atom());
      var v2 = new ShardKey(new Atom());
      Aver.IsTrue(ShardKey.Type.Atom == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueAtom, v2.ValueAtom);
      Aver.IsTrue(v1 == v2);

      "Value: {0}".SeeArgs(v1);//ToString()

      v1 = new ShardKey(Atom.Encode("abc1234"));
      v2 = new ShardKey(Atom.Encode("abc1234"));
      Aver.IsTrue(ShardKey.Type.Atom == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueAtom, v2.ValueAtom);
      Aver.IsTrue(v1 == v2);
      Aver.AreEqual(Atom.Encode("abc1234"), v1.ValueAtom);

      "Value: {0}".SeeArgs(v1);//ToString()

      v2 = new ShardKey(Atom.Encode("4321abc"));
      Aver.AreNotEqual(v1, v2);
      Aver.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreNotEqual(v1.Hash, v2.Hash);
      Aver.AreNotEqual(v1.ValueAtom, v2.ValueAtom);
      Aver.IsTrue(v1 != v2);
    }


    [Run]
    public void ShardKey_EntityId()
    {
      var v1 = new ShardKey(new EntityId());
      var v2 = new ShardKey(new EntityId());
      Aver.IsTrue(ShardKey.Type.EntityId == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueEntityId, v2.ValueEntityId);
      Aver.IsTrue(v1 == v2);

      "Value: {0}".SeeArgs(v1);//ToString()

      v1 = new ShardKey(EntityId.Parse("a@b::c"));
      v2 = new ShardKey(EntityId.Parse("a@b::c"));
      Aver.IsTrue(ShardKey.Type.EntityId == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueEntityId, v2.ValueEntityId);
      Aver.IsTrue(v1 == v2);
      Aver.AreEqual(EntityId.Parse("a@b::c"), v1.ValueEntityId);

      "Value: {0}".SeeArgs(v1);//ToString()

      v2 = new ShardKey(EntityId.Parse("another@b::c"));
      Aver.AreNotEqual(v1, v2);
      Aver.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreNotEqual(v1.Hash, v2.Hash);
      Aver.AreNotEqual(v1.ValueEntityId, v2.ValueEntityId);
      Aver.IsTrue(v1 != v2);
    }

    [Run]
    public void ShardKey_Ulong()
    {
      var v1 = new ShardKey(0ul);
      var v2 = new ShardKey(0ul);
      Aver.IsTrue(ShardKey.Type.Ulong == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueUlong, v2.ValueUlong);
      Aver.IsTrue(v1 == v2);

      "Value: {0}".SeeArgs(v1);//ToString()

      v1 = new ShardKey(1234ul);
      v2 = new ShardKey(1234ul);
      Aver.IsTrue(ShardKey.Type.Ulong == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueUlong, v2.ValueUlong);
      Aver.IsTrue(v1 == v2);
      Aver.AreEqual(1234ul, v1.ValueUlong);

      "Value: {0}".SeeArgs(v1);//ToString()

      v2 = new ShardKey(98766554321ul);
      Aver.AreNotEqual(v1, v2);
      Aver.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreNotEqual(v1.Hash, v2.Hash);
      Aver.AreNotEqual(v1.ValueUlong, v2.ValueUlong);
      Aver.IsTrue(v1 != v2);
    }

    [Run]
    public void ShardKey_Uint()
    {
      var v1 = new ShardKey(0u);
      var v2 = new ShardKey(0u);
      Aver.IsTrue(ShardKey.Type.Uint == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueUint, v2.ValueUint);
      Aver.IsTrue(v1 == v2);

      "Value: {0}".SeeArgs(v1);//ToString()

      v1 = new ShardKey(1234u);
      v2 = new ShardKey(1234u);
      Aver.IsTrue(ShardKey.Type.Uint == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueUint, v2.ValueUint);
      Aver.IsTrue(v1 == v2);
      Aver.AreEqual(1234u, v1.ValueUint);

      "Value: {0}".SeeArgs(v1);//ToString()

      v2 = new ShardKey(87872u);
      Aver.AreNotEqual(v1, v2);
      Aver.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreNotEqual(v1.Hash, v2.Hash);
      Aver.AreNotEqual(v1.ValueUint, v2.ValueUint);
      Aver.IsTrue(v1 != v2);
    }

    [Run]
    public void ShardKey_DateTime()
    {
      var v1 = new ShardKey(new DateTime(1980, 1, 1, 1, 1, 1, DateTimeKind.Utc));
      var v2 = new ShardKey(new DateTime(1980, 1, 1, 1, 1, 1, DateTimeKind.Utc));
      Aver.IsTrue(ShardKey.Type.DateTime == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueDateTime, v2.ValueDateTime);
      Aver.IsTrue(v1 == v2);
      Aver.AreEqual(new DateTime(1980, 1, 1, 1, 1, 1, DateTimeKind.Utc), v1.ValueDateTime);

      "Value: {0}".SeeArgs(v1);//ToString()

      v2 = new ShardKey(new DateTime(1980, 1, 1, 1, 1, 2, DateTimeKind.Utc));
      Aver.AreNotEqual(v1, v2);
      Aver.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreNotEqual(v1.Hash, v2.Hash);
      Aver.AreNotEqual(v1.ValueDateTime, v2.ValueDateTime);
      Aver.IsTrue(v1 != v2);
    }

    [Run]
    public void ShardKey_Guid()
    {
      var v1 = new ShardKey(Guid.Empty);
      var v2 = new ShardKey(Guid.Empty);
      Aver.IsTrue(ShardKey.Type.Guid == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueGuid, v2.ValueGuid);
      Aver.IsTrue(v1 == v2);

      "Value: {0}".SeeArgs(v1);//ToString()

      v1 = new ShardKey(Guid.Parse("{F52C9D55-E500-40F4-BF89-39FD6D8D9DC5}"));
      v2 = new ShardKey(Guid.Parse("{F52C9D55-E500-40F4-BF89-39FD6D8D9DC5}"));
      Aver.IsTrue(ShardKey.Type.Guid == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueGuid, v2.ValueGuid);
      Aver.IsTrue(v1 == v2);
      Aver.AreEqual(Guid.Parse("{F52C9D55-E500-40F4-BF89-39FD6D8D9DC5}"), v1.ValueGuid);

      "Value: {0}".SeeArgs(v1);//ToString()

      v2 = new ShardKey(Guid.Parse("{F52C9D55-E500-40F4-BF89-39FD6D8D9DCF}"));
      Aver.AreNotEqual(v1, v2);
      Aver.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreNotEqual(v1.Hash, v2.Hash);
      Aver.AreNotEqual(v1.ValueGuid, v2.ValueGuid);
      Aver.IsTrue(v1 != v2);
    }

    [Run]
    public void ShardKey_String()
    {
      var v1 = new ShardKey((string)null);
      var v2 = new ShardKey((string)null);
      Aver.IsTrue(ShardKey.Type.String == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueString, v2.ValueString);
      Aver.IsTrue(v1 == v2);

      "Value: {0}".SeeArgs(v1);//ToString()

      v1 = new ShardKey("");
      v2 = new ShardKey("");
      Aver.IsTrue(ShardKey.Type.String == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueString, v2.ValueString);
      Aver.IsTrue(v1 == v2);


      "Value: {0}".SeeArgs(v1);//ToString()

      v1 = new ShardKey("abcd12");
      v2 = new ShardKey("abcd12");
      Aver.IsTrue(ShardKey.Type.String == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.AreEqual(v1.ValueString, v2.ValueString);
      Aver.IsTrue(v1 == v2);
      Aver.AreEqual("abcd12", v1.ValueString);

      "Value: {0}".SeeArgs(v1);//ToString()

      v2 = new ShardKey("acbfegeddregheljg");
      Aver.AreNotEqual(v1, v2);
      Aver.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreNotEqual(v1.Hash, v2.Hash);
      Aver.AreNotEqual(v1.ValueString, v2.ValueString);
      Aver.IsTrue(v1 != v2);
    }


    [Run]
    public void ShardKey_ByteArray()
    {
      var v1 = new ShardKey((byte[])null);
      var v2 = new ShardKey((byte[])null);
      Aver.IsTrue(ShardKey.Type.ByteArray == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.IsTrue(v1.ValueByteArray.MemBufferEquals(v2.ValueByteArray));
      Aver.IsTrue(v1 == v2);

      "Value: {0}".SeeArgs(v1);//ToString()

      v1 = new ShardKey(new byte[0]);
      v2 = new ShardKey(new byte[0]);
      Aver.IsTrue(ShardKey.Type.ByteArray == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.IsTrue(v1.ValueByteArray.MemBufferEquals(v2.ValueByteArray));
      Aver.IsTrue(v1 == v2);

      "Value: {0}".SeeArgs(v1);//ToString()

      v1 = new ShardKey(new byte[]{1, 3, 9});
      v2 = new ShardKey(new byte[]{1, 3, 9});
      Aver.IsTrue(ShardKey.Type.ByteArray == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.IsTrue(v1.ValueByteArray.MemBufferEquals(v2.ValueByteArray));
      Aver.IsTrue(v1 == v2);

      Aver.AreEqual(3,  v1.ValueByteArray.Length);
      Aver.AreEqual(1, v1.ValueByteArray[0]);
      Aver.AreEqual(3, v1.ValueByteArray[1]);
      Aver.AreEqual(9, v1.ValueByteArray[2]);

      "Value: {0}".SeeArgs(v1);//ToString()

      v2 = new ShardKey(new byte[]{1, 3, 9, 11});
      Aver.AreNotEqual(v1, v2);
      Aver.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreNotEqual(v1.Hash, v2.Hash);
      Aver.IsFalse(v1.ValueByteArray.MemBufferEquals(v2.ValueByteArray));
      Aver.IsTrue(v1 != v2);
    }


    public class Cuztom : IDistributedStableHashProvider
    {
      public ulong Data{  get; set;}
      public ulong GetDistributedStableHash() => Data;
    }

    [Run]
    public void ShardKey_Object_IDistributedStableHashProvider()
    {
      var v1 = new ShardKey((object)null);
      var v2 = new ShardKey((object)null);
      Aver.IsTrue(ShardKey.Type.IDistributedStableHashProvider == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.IsTrue(v1.ValueIDistributedStableHashProvider == v2.ValueIDistributedStableHashProvider);
      Aver.IsTrue(v1 == v2);

      "Value: {0}".SeeArgs(v1);//ToString()

      var one = new Cuztom { Data = 123 };
      v1 = new ShardKey(one);
      v2 = new ShardKey(one);
      Aver.IsTrue(ShardKey.Type.IDistributedStableHashProvider == v1.DataType);
      Aver.AreEqual(v1, v2);
      Aver.AreEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);
      Aver.IsTrue(v1.ValueIDistributedStableHashProvider == v2.ValueIDistributedStableHashProvider);
      Aver.IsTrue(v1 == v2);

      "Value: {0}".SeeArgs(v1);//ToString()

      v2 = new ShardKey(new Cuztom { Data = 123 });//different instance
      Aver.AreNotEqual(v1, v2);
      Aver.AreNotEqual(v1.GetHashCode(), v2.GetHashCode());
      Aver.AreEqual(v1.Hash, v2.Hash);//these are still equal as data is the same!!!
      Aver.IsFalse(v1.ValueIDistributedStableHashProvider == v2.ValueIDistributedStableHashProvider);
      Aver.IsTrue(v1 != v2);

      v2 = new ShardKey(new Cuztom { Data = 124 });//different instance and different data
      Aver.AreNotEqual(v1.Hash, v2.Hash);//these are NOT equal as data is different
    }

  }
}
