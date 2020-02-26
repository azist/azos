/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System.Security.Cryptography;

namespace Azos.Platform.Abstraction.NetFramework
{
  internal class NetCryptography : IPALCryptography
  {
    public NetCryptography(){ }

    public byte[] ComputePBKDF2(byte[] pwd, byte[] salt, int byteSize, int iterations, HashAlgorithmName algorithm)
    {
      using(var kdf = new Rfc2898DeriveBytes(pwd, salt, iterations, algorithm))
       return kdf.GetBytes(byteSize);
    }
  }
}
