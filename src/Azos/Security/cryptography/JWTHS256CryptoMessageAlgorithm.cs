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
      m_HMACKey = BuildKeyFromConfig(config[CONFIG_HMAC_SECTION], 64);//HMAC SHA2 = 64 byte key
    //  The secret key for System.Security.Cryptography.HMACSHA256 encryption. The key
    //     can be any length. However, the recommended size is 64 bytes. If the key is more
    //     than 64 bytes long, it is hashed (using SHA-256) to derive a 64-byte key. If
    //     it is less than 64 bytes long, it is padded to 64 bytes.
    }

    protected override void Destructor()
    {
      Array.Clear(m_HMACKey, 0, m_HMACKey.Length);
      base.Destructor();
    }

    private byte[] m_HMACKey;


    public override CryptoMessageAlgorithmFlags Flags => CryptoMessageAlgorithmFlags.JWT | CryptoMessageAlgorithmFlags.CanUnprotect;

    public override string ProtectToString(ArraySegment<byte> originalMessage)
    {
      originalMessage.Array.NonNull(nameof(originalMessage));

      if (originalMessage.Count < 1)
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + "{0}.Protect(originalMessage.len < 1)".Args(GetType().Name));

      var b64payload = originalMessage.ToWebSafeBase64();

      var hdrpay = B64_HEADER + "." + b64payload;
      var binhdrpay = Encoding.UTF8.GetBytes(hdrpay);

      using (var hmac = new HMACSHA256(m_HMACKey))
      {
        var hash = hmac.ComputeHash(binhdrpay).ToWebSafeBase64();
        var fullSignedToken = hdrpay + "." + hash;
        return fullSignedToken;
      }
    }

    public override byte[] Protect(ArraySegment<byte> originalMessage)
    {
      var jwtString = ProtectToString(originalMessage);
      return Encoding.UTF8.GetBytes(jwtString);
    }

    public override byte[] UnprotectFromString(string protectedMessage)
    {
      protectedMessage.NonBlank(nameof(protectedMessage));
      if (protectedMessage.Length < B64_HEADER.Length + 1)
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + "{0}.Unprotect(protectedMessage.Count < hdr)".Args(GetType().Name));

      try
      {
        var segs = protectedMessage.Split('.');
        if (segs.Length != 3) return null;  //jwt: header.payload.sig

        var hdr = segs[0].FromWebSafeBase64();
        if (hdr.Length == 0) return null;
        var shdr = Encoding.UTF8.GetString(hdr);
        if (shdr.Length < HEADER.Length) return null;
        var hdrData = shdr.JsonToDataObject() as JsonDataMap;
        if (hdrData == null) return null;
        if (!hdrData["typ"].AsString().EqualsOrdIgnoreCase("JWT")) return null;
        if (!hdrData["alg"].AsString().EqualsOrdIgnoreCase("HS256")) return null;


        var hdrpay = segs[0] + "." + segs[1];
        var binhdrpay = Encoding.UTF8.GetBytes(hdrpay);

        using (var hmac = new HMACSHA256(m_HMACKey))
        {
          var ssig = segs[2].FromWebSafeBase64();
          var hash = hmac.ComputeHash(binhdrpay);
          if (!hash.MemBufferEquals(ssig)) return null;//hash mismatch
          var spayload = segs[1].FromWebSafeBase64();
          return spayload;
        }
      }
      catch(Exception error)
      {
        WriteLog(Log.MessageType.TraceErrors, nameof(UnprotectFromString), "Leaked on bad message: " + error.ToMessageWithType(), error);
        return null;
      }
    }

    public override byte[] Unprotect(ArraySegment<byte> protectedMessage)
    {
      protectedMessage.Array.NonNull(nameof(protectedMessage));

      string jwtString = null;
      try
      {
        jwtString = Encoding.UTF8.GetString(protectedMessage.Array, protectedMessage.Offset, protectedMessage.Count);
      }
      catch(Exception error)
      {
        WriteLog(Log.MessageType.TraceErrors, nameof(Unprotect), "Leaked on bad UTF8 decode: " + error.ToMessageWithType(), error);
        return null;
      }

      return UnprotectFromString(jwtString);
    }

  }
}
