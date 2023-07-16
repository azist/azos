/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Security.Cryptography;

using Azos.Conf;

namespace Azos.Security
{
  static partial class TheSafe
  {
    /// <summary>
    /// Password algorithm bases encryption/decryption on a single password phrase (no other keys required)
    /// </summary>
    public sealed class PwdAlgorithm : Algorithm
    {
      public PwdAlgorithm(string name, IConfigSectionNode config) : base(name, config)
      {
        config.NonEmpty(nameof(config));
        m_Password = config.ValOf("password", "pwd");
        m_Password.NonBlank("$pwd|$password");
      }

      private readonly string m_Password;


      public override byte[] Cipher(byte[] originalValue)
      {
        (originalValue.NonNull(nameof(originalValue)).Length > 0).IsTrue("Non empty");
        return TheSafe.Protect(originalValue, m_Password);
      }

      public override byte[] Decipher(byte[] protectedValue)
      {
        protectedValue.NonNull(nameof(protectedValue));
        return TheSafe.Unprotect(protectedValue, m_Password);
      }
    }//pwd
  }
}
