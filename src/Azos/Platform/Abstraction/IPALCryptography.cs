/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Azos.Platform.Abstraction
{
  /// <summary>
  /// Provides platform-specific cryptography functions
  /// </summary>
  public interface IPALCryptography
  {
    /// <summary>
    /// Computes Key Derivation based on the specified hashing
    /// </summary>
    byte[] ComputePBKDF2(byte[] pwd, byte[] salt, int byteSize, int iterations, HashAlgorithmName algorithm);
  }
}
