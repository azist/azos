/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Azos.Data;

namespace Azos.Security
{
  /// <summary>
  /// Facilitates process-global access to secrets such as passwords and crypto key strings.
  /// This class does not use application chassis and is PROCESS-INSTANCE-GLOBAL by design.
  /// </summary>
  public static partial class TheSafe
  {
    /// <summary>
    /// If this flag is set on the current call scope, authorizes access to safe
    /// decipher method, otherwise <see cref="DecipherConfigValue(string, bool, string)"/>
    /// would fail with <see cref="AccessToTheSafeDeniedException"/>
    /// </summary>
    public static readonly Atom SAFE_CONFIG_ACCESS_FLAG = Atom.Encode("safeconf");

    /// <summary>
    /// If this flag is set on the current call scope, authorizes access to safe
    /// decipher method, otherwise <see cref="Decipher(byte[], string)"/>
    /// would fail with <see cref="AccessToTheSafeDeniedException"/>
    /// </summary>
    public static readonly Atom SAFE_GENERAL_ACCESS_FLAG = Atom.Encode("safegen");


    /// <summary>
    /// Using the specified named algorithm deciphers a `byte[]` value encoded as a string (using `base64:` or byte array syntax)
    /// <see cref="StringValueConversion.AsByteArray(string, byte[])"/>
    /// into another `byte[]` value encoded as a string with `base64:` prefix.
    /// Returns null if value is invalid or algorithm is not found or mismatches the value.
    /// For security purposes this method does not throw exceptions explaining why the value has not been deciphered.
    /// An algorithm name can be null which means the default algorithm
    /// </summary>
    /// <param name="value">Config value to decipher. Null or empty returns null</param>
    /// <param name="toString">If true returns a string read from decoded byte[] using UTF8, otherwise returns base64-encoded byte array</param>
    /// <param name="algorithmName">Name of algorithm, if null then the default algorithm is assumed</param>
    public static string DecipherConfigValue(string value, bool toString, string algorithmName = null)
    {
      if (value.IsNullOrWhiteSpace()) return null;

      if (!SecurityFlowScope.CheckFlag(SAFE_CONFIG_ACCESS_FLAG))
        throw new AccessToTheSafeDeniedException(StringConsts.SECURITY_ACCESS_TO_THESAFE_DENIED_ERROR.Args($"{nameof(SecurityFlowScope)}({nameof(TheSafe)}.{nameof(SAFE_CONFIG_ACCESS_FLAG)})"));

      var ain = value.AsByteArray();

 //// Azos.Scripting.Conout.See(ain.ToHexDump());

      var aout = decipher(ain, algorithmName);

      if (aout == null) return null;

      if (toString)
        return PROTECTION_STRING_ENCODING.GetString(aout);
      else
        return StringValueConversion.BASE64_VALUE_PREFIX + aout.ToWebSafeBase64();
    }

    /// <summary>
    /// Using the specified named algorithm deciphers the value.
    /// Returns null if value is invalid or algorithm is not found or mismatches the value.
    /// For security purposes this method does not throw exceptions explaining why the value has not been deciphered.
    /// A null algorithm name means the default algorithm
    /// </summary>
    public static byte[] Decipher(byte[] value, string algorithmName = null)
    {
      if (value == null) return null;

      if (!SecurityFlowScope.CheckFlag(SAFE_GENERAL_ACCESS_FLAG))
        throw new AccessToTheSafeDeniedException(StringConsts.SECURITY_ACCESS_TO_THESAFE_DENIED_ERROR.Args($"{nameof(SecurityFlowScope)}({nameof(TheSafe)}.{nameof(SAFE_GENERAL_ACCESS_FLAG)})"));

      return decipher(value, algorithmName);
    }

    /// <summary>
    /// Using the specified named algorithm deciphers the value into string.
    /// Returns null if value is invalid or algorithm is not found or mismatches the value.
    /// For security purposes this method does not throw exceptions explaining why the value has not been deciphered.
    /// A null algorithm name means the default algorithm
    /// </summary>
    public static string DecipherText(byte[] value, string algorithmName = null)
    {
      var raw = Decipher(value, algorithmName);
      if (raw == null) return null;

      using var ms = new MemoryStream(raw);
      using var r = new StreamReader(ms, true);
      return r.ReadToEnd();
    }

    /// <summary>
    /// Using the specified algorithm ciphers a config text value into ciphered `byte[]` value
    /// encoded using a `base64:` prefix. Null returned for null input string
    /// </summary>
    public static string CipherConfigValue(string value, string algorithmName = null)
    {
      var aout = CipherText(value, algorithmName);
      if (aout == null) return null;
      return StringValueConversion.BASE64_VALUE_PREFIX + aout.ToWebSafeBase64();
    }

    /// <summary>
    /// Using the specified algorithm ciphers a config binary value into ciphered `byte[]` value
    /// encoded using a `base64:` prefix. Null returned for null input byte buffer
    /// </summary>
    public static string CipherConfigValue(byte[] value, string algorithmName = null)
    {
      var aout = Cipher(value, algorithmName);
      if (aout == null) return null;
      return StringValueConversion.BASE64_VALUE_PREFIX + aout.ToWebSafeBase64();
    }

    /// <summary>
    /// Using the specified algorithm name, ciphers the byte buffer value.
    /// Null returned for null input byte buffer
    /// </summary>
    public static byte[] Cipher(byte[] value, string algorithmName = null)
    {
      if (value == null) return null;

      if (!SecurityFlowScope.CheckFlag(SAFE_GENERAL_ACCESS_FLAG))
        throw new AccessToTheSafeDeniedException(StringConsts.SECURITY_ACCESS_TO_THESAFE_DENIED_ERROR.Args($"{nameof(SecurityFlowScope)}({nameof(TheSafe)}.{nameof(SAFE_GENERAL_ACCESS_FLAG)})"));

      return cipher(value, algorithmName);
    }

    /// <summary>
    /// Using the spcified algo name ciphers a string value into a byte[].
    /// Null returned for null string
    /// </summary>
    public static byte[] CipherText(string value, string algorithmName = null)
    {
      if (value == null) return null;
      var raw = PROTECTION_STRING_ENCODING.GetBytes(value);
      var result = Cipher(raw, algorithmName);
      return result;
    }

    private static byte[] decipher(byte[] value, string algorithmName)
    {
      if (value == null) return null;
      var algo = getAlgorithmOrDefault(algorithmName);
      if (algo == null) return null;
      var result = algo.Decipher(value);
      return result;
    }

    private static byte[] cipher(byte[] value, string algorithmName)
    {
      if (value == null) return null;
      var algo = getAlgorithmOrDefault(algorithmName);
      if (algo == null) return null;
      var result = algo.Cipher(value);
      return result;
    }
  }
}
