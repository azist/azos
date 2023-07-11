/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;

namespace Azos.Security
{
  /// <summary>
  /// Facilitates process-global access to secrets such as passwords and crypto key strings.
  /// This class does not use application chassis and is PROCESS-INSTANCE-GLOBAL by design.
  /// </summary>
  public static class TheSafe
  {
    /// <summary>
    /// If this flag is set on the current call scope, authorizes access to safe
    /// decipher method, otherwise <see cref="DecipherConfigValue(string, string)"/>
    /// would fail with <see cref="AccessToTheSafeDeniedException"/>
    /// </summary>
    public static readonly Atom SAFE_CONFIG_ACCESS_FLAG = Atom.Encode("safeconf");

    /// <summary>
    /// If this flag is set on the current call scope, authorizes access to safe
    /// decipher method, otherwise <see cref="DecipherConfigValue(string, string)"/>
    /// would fail with <see cref="AccessToTheSafeDeniedException"/>
    /// </summary>
    public static readonly Atom SAFE_GENERAL_ACCESS_FLAG = Atom.Encode("safegen");


    /// <summary>
    /// Using the specified named key deciphers a `byte[]` value encoded as a string (using `base64:` or byte array syntax)
    /// <see cref="StringValueConversion.AsByteArray(string, byte[])"/>
    /// into another `byte[]` value encoded as a string with `base64:` prefix.
    /// Returns null if value is invalid or key is not found or mismatches the value.
    /// For security purposes this method does not throw exceptions explaining why the value has not been deciphered
    /// </summary>
    public static string DecipherConfigValue(string key, string value)
    {
      if (value.IsNullOrWhiteSpace()) return null;
      key.NonBlank(nameof(key));

      if (!SecurityFlowScope.CheckFlag(SAFE_CONFIG_ACCESS_FLAG))
        throw new AccessToTheSafeDeniedException(StringConsts.SECURITY_ACCESS_TO_THESAFE_DENIED_ERROR.Args($"{nameof(SecurityFlowScope)}({nameof(TheSafe)}.{nameof(SAFE_CONFIG_ACCESS_FLAG)})"));

      var ain = value.AsByteArray();
      var aout = decipher(key, ain);
      return StringValueConversion.BASE64_VALUE_PREFIX + ain.ToWebSafeBase64();
    }

    /// <summary>
    /// Using the specified named key deciphers the value.
    /// Returns null if value is invalid or key is not found or mismatches the value.
    /// For security purposes this method does not throw exceptions explaining why the value has not been deciphered
    /// </summary>
    public static byte[] Decipher(string key, byte[] value)
    {
      if (value == null) return null;
      key.NonBlank(nameof(key));

      if (!SecurityFlowScope.CheckFlag(SAFE_GENERAL_ACCESS_FLAG))
        throw new AccessToTheSafeDeniedException(StringConsts.SECURITY_ACCESS_TO_THESAFE_DENIED_ERROR.Args($"{nameof(SecurityFlowScope)}({nameof(TheSafe)}.{nameof(SAFE_GENERAL_ACCESS_FLAG)})"));

      return decipher(key, value);
    }

    private static byte[] decipher(string key, byte[] value)
    {
      if (value == null) return null;


      return null;
    }


  }
}
