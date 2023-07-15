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

    [Run]
    public void ByteRoundtrip_01()
    {
      var bytes = Ambient.Random.NextRandomBytes(1025);
      var got = TheSafe.Protect(bytes, "1234567890");
      "ENCRYPTED: \n {0} \n".SeeArgs(got.ToHexDump());
      var bytes2 = TheSafe.Unprotect(got, "1234567890");
      Aver.IsTrue(bytes.MemBufferEquals(bytes2));
    }



  }
}


