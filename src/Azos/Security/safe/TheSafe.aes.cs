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
    /// Protects/unprotects payload using HMAC and AES
    /// </summary>
    public sealed class AesAlgorithm : Algorithm
    {
      private const int IV_LEN = 128 / 8;
      private const int HMAC_LEN = 256 / 8;
      private const int HDR_LEN = IV_LEN + HMAC_LEN;

      public const string CONFIG_HMAC_SECTION = "hmac";
      public const string CONFIG_AES_SECTION = "aes";

      public AesAlgorithm(IConfigSectionNode config) : base(config.NonEmpty(nameof(config)).ValOf(Configuration.CONFIG_NAME_ATTR), config)
      {
        m_HMACKeys = BuildKeysFromConfig(config, CONFIG_HMAC_SECTION, 64);//HMAC SHA2 = 64 byte key
        m_AESKeys = BuildKeysFromConfig(config, CONFIG_AES_SECTION, 256 / 8);//AES256 = 256 bit key
      }

      private byte[][] m_HMACKeys;
      private byte[][] m_AESKeys;


      public override byte[] Cipher(byte[] originalValue)
      {
        (originalValue.NonNull(nameof(originalValue)).Length > 0).IsTrue("Non empty");

        var iv = GenerateRandomBytes(IV_LEN);
        var keys = getKeys(iv);

        //https://crypto.stackexchange.com/questions/202/should-we-mac-then-encrypt-or-encrypt-then-mac
        var hmac = getHMAC(keys.hmac, new ArraySegment<byte>(iv), originalValue);

        using (var aes = makeAES())
        {
          using (var encryptor = aes.CreateEncryptor(keys.aes, iv))
          {
            var encrypted = encryptor.TransformFinalBlock(originalValue, 0, originalValue.Length);

            var result = new byte[HDR_LEN + encrypted.Length];
            Array.Copy(iv, 0, result, 0, IV_LEN);
            Array.Copy(hmac, 0, result, IV_LEN, HMAC_LEN);
            Array.Copy(encrypted, 0, result, HDR_LEN, encrypted.Length);

            return result;
          }
        }
      }

      public override byte[] Decipher(byte[] protectedValue)
      {
        protectedValue.NonNull(nameof(protectedValue));

        try
        {
          if (protectedValue.Length < HDR_LEN + 1) return null;

          var iv = new byte[IV_LEN];
          var hmac = new byte[HMAC_LEN];
          Array.Copy(protectedValue, 0, iv, 0, IV_LEN);
          Array.Copy(protectedValue, IV_LEN, hmac, 0, HMAC_LEN);
          var keys = getKeys(iv);

          using (var aes = makeAES())
          {
            using (var decrypt = aes.CreateDecryptor(keys.aes, iv))
            {
              var decrypted = decrypt.TransformFinalBlock(protectedValue, HDR_LEN, protectedValue.Length - HDR_LEN);

              //rehash locally and check
              var rehmac = getHMAC(keys.hmac, new ArraySegment<byte>(iv), new ArraySegment<byte>(decrypted));
              if (!hmac.MemBufferEquals(rehmac)) return null;//HMAC mismatch: message has been tampered with

              return decrypted;
            }
          }
        }
        catch
        {
          //WARNING: NEVER disclose the REASON of error to the caller, just return null
          return null;
        }
      }

      private AesManaged makeAES()
      {
        var aes = new AesManaged();
        aes.Mode = CipherMode.CBC;//Cipher Block Chaining requires random 128bit IV
        aes.KeySize = 256;
        aes.Padding = PaddingMode.PKCS7;//use Zeros, PKCS7 is dangerous with CBC AZ #759
        return aes;
      }

      private byte[] getHMAC(byte[] keyHMAC, ArraySegment<byte> nonce, ArraySegment<byte> data)
      {
        using (var ihash = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, keyHMAC))
        {
          ihash.AppendData(nonce.Array, nonce.Offset, nonce.Count);
          ihash.AppendData(data.Array, data.Offset, data.Count);
          return ihash.GetHashAndReset();
        }
      }

      //key rotation uses different keys based on IV
      //the IV is random for every call (message instance), consequently the choice of keys is random
      //for every call
      private (byte[] hmac, byte[] aes) getKeys(byte[] iv)
      {
        var nonce = iv[0] ^ iv[iv.Length - 1];
        var ihmac = nonce % m_HMACKeys.Length;
        var iaes = nonce % m_AESKeys.Length;
        return (m_HMACKeys[ihmac], m_AESKeys[iaes]);
      }
    }//aes
  }
}
