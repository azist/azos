/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Linq;
using System.Text;

using Azos.Serialization.JSON;

namespace Azos.Security
{
  /// <summary>
  /// Provides extension methods for working with crypto-protected messages
  /// </summary>
  public static class CryptoMessageUtils
  {
    /// <summary>
    /// Returns a first matching public cipher algorithm marked as the default one
    /// </summary>
    public static ICryptoMessageAlgorithm GetDefaultPublicCipher(this ISecurityManager secman)
     => secman.NonNull(nameof(secman))
              .Cryptography
              .MessageProtectionAlgorithms
              .FirstOrDefault( a => a.IsDefault &&
                                    a.Audience == CryptoMessageAlgorithmAudience.Public &&
                                    a.Flags.HasFlag(CryptoMessageAlgorithmFlags.Cipher))
              .NonNull("no Default Public Cipher algorithm configured");

    /// <summary>
    /// Returns a first matching public JWT algorithm marked as the default one
    /// </summary>
    public static ICryptoMessageAlgorithm GetDefaultPublicJWT(this ISecurityManager secman)
     => secman.NonNull(nameof(secman))
              .Cryptography
              .MessageProtectionAlgorithms
              .FirstOrDefault(a => a.IsDefault &&
                                   a.Audience == CryptoMessageAlgorithmAudience.Public &&
                                   a.Flags.HasFlag(CryptoMessageAlgorithmFlags.JWT))
              .NonNull("no Default Public JWT algorithm configured");

    /// <summary>
    /// Protects a message with Default public cipher
    /// </summary>
    /// <param name="secman">ISecurityManager</param>
    /// <param name="message">Message to protect using Json format</param>
    /// <param name="options">Json format options</param>
    /// <returns>Web-safe base64-encoded string representation of protected message suitable for direct use in Uris</returns>
    public static string PublicProtectAsString(this ISecurityManager secman, object message, JsonWritingOptions options = null)
    {
      var algo = secman.GetDefaultPublicCipher();
      var result = algo.ProtectAsString(message, options);
      return result;
    }

    /// <summary>
    /// Tries to decode/unprotect the message, returning null if the protectedMessage does not represent a valid protected message
    /// </summary>
    /// <param name="secman">ISecurityManager</param>
    /// <param name="protectedMessage">Protected message content encoded as string</param>
    /// <returns>Unprotected/decoded message or null if the protectedMessage is not valid</returns>
    public static IJsonDataObject PublicUnprotect(this ISecurityManager secman, string protectedMessage)
    {
      var algo = secman.GetDefaultPublicCipher();
      var result = algo.Unprotect(protectedMessage);
      return result;
    }

    /// <summary>
    /// Tries to decode/unprotect the object/map message, returning null if the protectedMessage does not represent a valid protected message
    /// </summary>
    /// <param name="secman">ISecurityManager</param>
    /// <param name="protectedMessage">Protected message content encoded as string</param>
    /// <returns>Unprotected/decoded object/map message or null if the protectedMessage is not valid</returns>
    public static JsonDataMap PublicUnprotectMap(this ISecurityManager secman, string protectedMessage)
    {
      var algo = secman.GetDefaultPublicCipher();
      var result = algo.Unprotect(protectedMessage);
      return result as JsonDataMap;
    }

    /// <summary>
    /// Protects message into a byte[]. Null is returned for null messages
    /// </summary>
    /// <param name="algorithm">Algorithm to use</param>
    /// <param name="originalMessage">Message to protect using Json format</param>
    /// <param name="options">Json format options</param>
    /// <returns>Binary representation of protected message</returns>
    public static byte[] ProtectAsBuffer(this ICryptoMessageAlgorithm algorithm, object originalMessage, JsonWritingOptions options = null)
    {
      if (originalMessage == null) return null;
      if (options==null) options = JsonWritingOptions.CompactRowsAsMap;
      var raw = JsonWriter.WriteToBuffer(originalMessage, options, Encoding.UTF8);
      var result = algorithm.NonNull(nameof(algorithm)).Protect(new ArraySegment<byte>(raw));
      return result;
    }

    /// <summary>
    /// Protects message as web-safe URI string. Null is returned for null messages
    /// </summary>
    /// <param name="algorithm">Algorithm to use</param>
    /// <param name="originalMessage">Message to protect using Json format</param>
    /// <param name="options">Json format options</param>
    /// <returns>Web-safe base64-encoded string representation of protected message suitable for direct use in Uris</returns>
    public static string ProtectAsString(this ICryptoMessageAlgorithm algorithm, object originalMessage, JsonWritingOptions options = null)
    {
      if (originalMessage == null) return null;
      if (options == null) options = JsonWritingOptions.CompactRowsAsMap;
      var raw = JsonWriter.WriteToBuffer(originalMessage, options, Encoding.UTF8);
      var result = algorithm.NonNull(nameof(algorithm)).ProtectToString(new ArraySegment<byte>(raw));
      return result;
    }


    /// <summary>
    /// Tries to decode/unprotect the message, returning null if the protectedMessage does not represent a valid protected message
    /// </summary>
    /// <param name="algorithm">Algorithm to use</param>
    /// <param name="protectedMessage">Protected message content encoded as string</param>
    /// <returns>Unprotected/decoded message or null if the protectedMessage is not valid</returns>
    public static IJsonDataObject Unprotect(this ICryptoMessageAlgorithm algorithm, string protectedMessage)
    {
      var raw = algorithm.UnprotectFromString(protectedMessage);
      if (raw == null) return null;
      using (var ms = new MemoryStream(raw))
      {
        try
        {
          return JsonReader.DeserializeDataObject(ms, Encoding.UTF8, true);
        }
        catch
        {
          return null;//corrupted message
        }
      }
    }

    /// <summary>
    /// Tries to decode/unprotect the message, returning null if the protectedMessage does not represent a valid protected message
    /// </summary>
    /// <param name="algorithm">Algorithm to use</param>
    /// <param name="protectedMessage">Protected message content</param>
    /// <returns>Unprotected/decoded message or null if the protectedMessage is not valid</returns>
    public static IJsonDataObject Unprotect(this ICryptoMessageAlgorithm algorithm, ArraySegment<byte> protectedMessage)
    {
      var raw = algorithm.Unprotect(protectedMessage);
      if (raw==null) return null;
      using(var ms = new MemoryStream(raw))
      {
        try
        {
          return JsonReader.DeserializeDataObject(ms, Encoding.UTF8, true);
        }
        catch
        {
          return null;//corrupted message
        }
      }
    }



    public static string PublicProtectJWTPayload(this ISecurityManager secman, JsonDataMap jwtPayload)
    {
      var algo = secman.GetDefaultPublicJWT();
      var result = ProtectJWTPayloadAsString(algo, jwtPayload);
      return result;
    }

    public static JsonDataMap PublicUnprotectJWTPayload(this ISecurityManager secman, string jwtPayload)
    {
      var algo = secman.GetDefaultPublicJWT();
      var result = UnprotectJWTPayload(algo, jwtPayload);
      return result;
    }


    public static byte[] ProtectJWTPayloadAsBuffer(this ICryptoMessageAlgorithm algorithm, JsonDataMap payload)
    {
      var binPayload = JsonWriter.WriteToBuffer(payload.NonNull(nameof(payload)), JsonWritingOptions.CompactRowsAsMap, Encoding.UTF8);
      return algorithm.Protect(new ArraySegment<byte>(binPayload));
    }

    public static string ProtectJWTPayloadAsString(this ICryptoMessageAlgorithm algorithm, JsonDataMap payload)
    {
      var binPayload = JsonWriter.WriteToBuffer(payload.NonNull(nameof(payload)), JsonWritingOptions.CompactRowsAsMap, Encoding.UTF8);
      return algorithm.ProtectToString(new ArraySegment<byte>(binPayload));
    }

    public static JsonDataMap UnprotectJWTPayload(this ICryptoMessageAlgorithm algorithm, string protectedMessage)
    {
      var raw = algorithm.UnprotectFromString(protectedMessage);
      if (raw == null) return null;
      using (var ms = new MemoryStream(raw))
      {
        try
        {
          return JsonReader.DeserializeDataObject(ms, Encoding.UTF8, true) as JsonDataMap;
        }
        catch
        {
          return null;//corrupted message
        }
      }

    }

    public static JsonDataMap ProtectJWTPayloadAsString(this ICryptoMessageAlgorithm algorithm, byte[] protectedMessage)
    {
      var raw = algorithm.Unprotect(new ArraySegment<byte>(protectedMessage));
      if (raw == null) return null;
      using (var ms = new MemoryStream(raw))
      {
        try
        {
          return JsonReader.DeserializeDataObject(ms, Encoding.UTF8, true) as JsonDataMap;
        }
        catch
        {
          return null;//corrupted message
        }
      }
    }


  }
}
