/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


using Azos.Conf;
using Azos.Data;

using Azos.Serialization.JSON;

namespace Azos.Security
{
  /// <summary>
  /// Provides implementation of a JWT message protection using HMACSHA256.
  /// The supplied payload represents UTF8-encoded JWT claims
  /// </summary>
  public sealed class JWTHS256CryptoMessageAlgorithm : CryptoMessageAlgorithm
  {
    private const string HEADER = "{\"typ\":\"JWT\",\"alg\":\"HS256\"}";
    private static readonly string B64_HEADER = Encoding.UTF8.GetBytes(HEADER).ToWebSafeBase64();

    public const string CONFIG_HMAC_SECTION = "hmac";

    public JWTHS256CryptoMessageAlgorithm(ICryptoManagerImplementation director, IConfigSectionNode config) : base(director, config)
    {
      //m_HMACKeys = BuildKeysFromConfig(config, CONFIG_HMAC_SECTION, 64);//HMAC SHA2 = 64 byte key
    }

    private byte[] m_HMACKey;


    public override CryptoMessageAlgorithmFlags Flags => CryptoMessageAlgorithmFlags.Cipher | CryptoMessageAlgorithmFlags.CanUnprotect;


    public override byte[] Protect(ArraySegment<byte> originalMessage)
    {
      originalMessage.Array.NonNull(nameof(originalMessage));

      if (originalMessage.Count < 1)
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + "{0}.Protect(originalMessage.len < 1)".Args(GetType().Name));

      var b64claims = originalMessage.ToWebSafeBase64();

      var payload = B64_HEADER + "." + b64claims;
      var binpayload = Encoding.UTF8.GetBytes(payload);

      using(var hmac = new HMACSHA256(m_HMACKey))
      {
        var hash = hmac.ComputeHash(binpayload).ToWebSafeBase64();
        var signed = payload + "." + hash;
        return Encoding.UTF8.GetBytes(signed);
      }
    }

    public override byte[] Unprotect(ArraySegment<byte> protectedMessage)
    {
      protectedMessage.Array.NonNull(nameof(protectedMessage));
      if (protectedMessage.Count < B64_HEADER.Length + 1)
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + "{0}.Unprotect(protectedMessage.Count < hdr)".Args(GetType().Name));

      var token = Encoding.UTF8.GetString(protectedMessage.Array, protectedMessage.Offset, protectedMessage.Count);
      var segs = token.Split('.');
      if (segs.Length!=3) return null;  //jwt: header.claims.sig

      var hdr = segs[0].FromWebSafeBase64();
      if (hdr.Length == 0) return null;
      var shdr = Encoding.UTF8.GetString(hdr);
      if (shdr.Length < HEADER.Length) return null;
      var hdrData = shdr.JsonToDataObject() as JsonDataMap;
      if (hdrData==null) return null;
      if (!hdrData["typ"].AsString().EqualsOrdIgnoreCase("JWT")) return null;
      if (!hdrData["alg"].AsString().EqualsOrdIgnoreCase("HS256")) return null;


      var payload = segs[0] + "." + segs[1];
      var binpayload = Encoding.UTF8.GetBytes(payload);

      using (var hmac = new HMACSHA256(m_HMACKey))
      {
        var ssig = segs[2].FromWebSafeBase64();
        var hash = hmac.ComputeHash(binpayload);
        if (!hash.MemBufferEquals(ssig)) return null;//hash mismatch
        var sclaims = segs[1].FromWebSafeBase64();
        return sclaims;
      }
    }

  }
}
