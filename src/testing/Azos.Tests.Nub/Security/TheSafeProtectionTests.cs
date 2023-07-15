/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data;
using Azos.Scripting;
using Azos.Security;

namespace Azos.Tests.Nub.Security
{
  [Runnable]
  public class TheSafeProtectionTests
  {
    [Run]
    public void StringRoundtrip_01()
    {
      var str = "My Message";
      var got = TheSafe.ProtectString(str, "1234");
      "ENCRYPTED: \n {0} \n".SeeArgs(got.ToHexDump());
      var str2 = TheSafe.UnprotectString(got, "1234");
      Aver.AreEqual(str, str2);
    }

    [Run("!safe-all-lengths", "count=1500")]
    public void StringSizes_01(int count)
    {
      for(var i = 1; i <= count; i++)
      {
        if (!Azos.Apps.ExecutionContext.Application.Active) break;
        var str = new string('x', i);
        var pwd = Ambient.Random.NextRandomWebSafeString(16, 16);
        var got = TheSafe.ProtectString(str, pwd);
        var str2 = TheSafe.UnprotectString(got, pwd);
        Aver.AreEqual(str, str2);
        if (i%10==0) i.See();
      }
    }

    [Run]
    public void ByteRoundtrip_01()
    {
      var bytes = Ambient.Random.NextRandomBytes(256);
      var got = TheSafe.Protect(bytes, "1234567890");
      "ENCRYPTED: \n {0} \n".SeeArgs(got.ToHexDump());
      var bytes2 = TheSafe.Unprotect(got, "1234567890");
      Aver.IsTrue(bytes.MemBufferEquals(bytes2));
    }

    [Run]
    public void ByteRoundtrip_02()
    {
      var bytes = Ambient.Random.NextRandomBytes(789);
      var got1 = TheSafe.Protect(bytes, "1234567890");
      var got2 = TheSafe.Protect(bytes, "1234567890");
      var got3 = TheSafe.Protect(bytes, "abc");
      Aver.IsFalse(got1.MemBufferEquals(got2));
      Aver.IsFalse(got1.MemBufferEquals(got3));

      var back1 = TheSafe.Unprotect(got1, "1234567890");
      var back2 = TheSafe.Unprotect(got2, "1234567890");
      var back3 = TheSafe.Unprotect(got3, "abc");
      Aver.IsTrue(bytes.MemBufferEquals(back1));
      Aver.IsTrue(bytes.MemBufferEquals(back2));
      Aver.IsTrue(bytes.MemBufferEquals(back3));
    }

    [Run]
    public void ByteRoundtrip_03()
    {
      var bytes = Ambient.Random.NextRandomBytes(5 * 1024 * 1024);
      var got = TheSafe.Protect(bytes,    "1234567890");
      var bytes2 = TheSafe.Unprotect(got, "1234567890");
      Aver.IsTrue(bytes.MemBufferEquals(bytes2));

      Aver.IsNull(TheSafe.Unprotect(got, "123456789"));
      Aver.IsNull(TheSafe.Unprotect(got, "0123456789"));

    }



  }
}


