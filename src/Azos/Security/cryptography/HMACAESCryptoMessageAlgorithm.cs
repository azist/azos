/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Linq;
using System.Security.Cryptography;

using Azos.Conf;
using Azos.Data;

namespace Azos.Security
{
  /// <summary>
  /// Provides implementation of a message protection algorithm based on a combination of AES256 CBC mode cipher
  /// and HMACSHA256 message authenticity protection. The algorithm ensures that the data is encrypted AND can be
  /// decrypted back yielding the same HMAC, hence the algorithm uses multiple pairs (at least 2) of private keys:
  ///  one for AES256 cipher, and another one for HMAC authenticity check on decipher.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A message to be Protect()-ed is supplied as a byte[].
  /// First, we generate a crypto random IV of 128 bits, then we computer HMAC(iv + originalMsg, hmac_keyX).
  /// Notice, that the HMAC is based on the originalMessage AND the IV which is random for every call.
  /// An attacker may not "recreate" the original payload from hash (ensured by one-way HMAC).
  /// </para>
  /// The protected message is thus:
  /// <code>
  ///  [IV 16 bytes][HMACSHA256(pvt1,IV + msg) 32 bytes][AES256.CBC(pvt2, msg)]
  /// </code>
  /// The same unprotected payload yields different protected content due to the usage of random IV, avalanche effect of HMAC, and CBC mode of AES using that IV.
  ///
  /// The Unprotect() phase repeats the process in reverse, applying AES256 decipher using the IV,
  /// then re-computes the HMAC(IV, deciphered_msg) to ensure that deciphered byte[] re-hashes into the same HMAC for its private key.
  ///
  /// Multiple Keys
  /// <para>
  /// Key rotation ensures that an attacker may not assume that the algorithm uses the same key for every message,
  /// furthermore there are 2 different keys used for every message protection and their inter-relationship may not be inferred either.
  /// Key selector functions uses different keys based on unpredictable IV/nonce,
  /// even though IV is transmitted in the clear, and this code is in the public domain
  /// a hacker has no way of knowing how many keys this algorithm instance is configured to use.
  /// This ensures that any kind of cracking is orders of magnitude harder as no assumptions
  /// about the number of keys or which key is used for what case can be made mathematically.
  /// What is more interesting, is that there is no 1:1 relationship between HMAC and AES keys in this
  /// scheme as one may have, as an example, 3 HMAC keys and 7 AES keys, therefore it is impossible
  /// to know which HMAC key is paired with which AES key.
  /// The key selector also provides additional protection property: if some keys ever get cracked
  /// the hacker will only get access to 1/x of the system x being the number of AES keys in the set.
  /// </para>
  /// </remarks>
  public sealed class HMACAESCryptoMessageAlgorithm : CryptoMessageAlgorithm
  {
    private const int IV_LEN = 128 / 8;
    private const int HMAC_LEN = 256 / 8;
    private const int HDR_LEN = IV_LEN + HMAC_LEN;

    public const string CONFIG_HMAC_SECTION = "hmac";
    public const string CONFIG_AES_SECTION = "aes";
    public const string CONFIG_KEY_ATTR = "key";

    public HMACAESCryptoMessageAlgorithm(ICryptoManagerImplementation director, IConfigSectionNode config) : base(director, config)
    {
      m_HMACKeys = buildKeys(config, CONFIG_HMAC_SECTION, 64);//HMAC SHA2 = 64 byte key
      m_AESKeys = buildKeys(config, CONFIG_AES_SECTION, 256 / 8);//AES256 = 256 bit key
    }

    private byte[][] buildKeys(IConfigSectionNode config, string sectionName, int len)
    {
      var result =  config.Children
                          .Where(c => c.IsSameName(sectionName) && c.ValOf(CONFIG_KEY_ATTR).IsNotNullOrWhiteSpace())
                          .Select(c => c.AttrByName(CONFIG_KEY_ATTR).ValueAsByteArray(null) ??
                                       throw new SecurityException("{0} config section `{1}` does not contain valid key byte array".Args(GetType().Name, c.RootPath)))
                          .ToArray();
      if (result.Length==0) throw new SecurityException("{0} config section `{1}` must contain at least one key entry".Args(GetType().Name, sectionName));

      foreach(var a in result)
      {
        if (a.Length!=len) throw new SecurityException("{0} config section `{1}` all keys must be of {2} bytes in length".Args(GetType().Name, sectionName, len));
        if (result.Any(a2 => a2!=a && a.MemBufferEquals(a2)))
             throw new SecurityException("{0} config section `{1}` contains duplicate keys".Args(GetType().Name, sectionName));
      }

      return result;
    }

    private byte[][] m_HMACKeys;
    private byte[][] m_AESKeys;


    public override CryptoMessageAlgorithmFlags Flags => CryptoMessageAlgorithmFlags.Cipher | CryptoMessageAlgorithmFlags.CanUnprotect;


    public override byte[] Protect(ArraySegment<byte> originalMessage)
    {
      originalMessage.Array.NonNull(nameof(originalMessage));

      if (originalMessage.Count < 1)
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + "{0}.Protect(originalMessage.len < 1)".Args(GetType().Name));

      var iv = ComponentDirector.GenerateRandomBytes(IV_LEN);
      var keys = getKeys(iv);
      var hmac = getHMAC(keys.hmac, new ArraySegment<byte>(iv), originalMessage);

      using (var aes = makeAES())
      {
        using (var encryptor = aes.CreateEncryptor(keys.aes, iv))
        {
          var encrypted = encryptor.TransformFinalBlock(originalMessage.Array, originalMessage.Offset, originalMessage.Count);

          var result = new byte[HDR_LEN + encrypted.Length];
          Array.Copy(iv,        0, result,       0, IV_LEN);
          Array.Copy(hmac,      0, result,  IV_LEN, HMAC_LEN);
          Array.Copy(encrypted, 0, result, HDR_LEN, encrypted.Length);

          return result;
        }
      }
    }

    public override byte[] Unprotect(ArraySegment<byte> protectedMessage)
    {
      protectedMessage.Array.NonNull(nameof(protectedMessage));
      if (protectedMessage.Count < HDR_LEN + 1)
        throw new SecurityException(StringConsts.ARGUMENT_ERROR + "{0}.Unprotect(protectedMessage.Count < {1})".Args(GetType().Name, HDR_LEN));

      var iv = new byte[IV_LEN];
      var hmac = new byte[HMAC_LEN];
      Array.Copy(protectedMessage.Array, protectedMessage.Offset, iv, 0, IV_LEN);
      Array.Copy(protectedMessage.Array, protectedMessage.Offset + IV_LEN, hmac, 0, HMAC_LEN);
      var keys = getKeys(iv);

      using (var aes = makeAES())
      {
        using (var decrypt = aes.CreateDecryptor(keys.aes, iv))
        {
          var decrypted = decrypt.TransformFinalBlock(protectedMessage.Array, protectedMessage.Offset + HDR_LEN, protectedMessage.Count - HDR_LEN);

          //rehash locally and check
          var rehmac = getHMAC(keys.hmac, new ArraySegment<byte>(iv), new ArraySegment<byte>(decrypted));
          if (!hmac.MemBufferEquals(rehmac)) return null;//HMAC mismatch: message has been tampered with

          return decrypted;
        }
      }
    }

    private AesManaged makeAES()
    {
      var aes = new AesManaged();
      aes.Mode = CipherMode.CBC;//Cipher Block Chaining requires random 128bit IV
      aes.KeySize = 256;
      aes.Padding = PaddingMode.PKCS7;
      return aes;
    }

    private byte[] getHMAC(byte[] keyHMAC, ArraySegment<byte> nonce, ArraySegment<byte> data)
    {
      using(var ihash = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, keyHMAC))
      {
       ihash.AppendData(nonce.Array, nonce.Offset, nonce.Count);
       ihash.AppendData(data.Array, data.Offset, data.Count);
       return ihash.GetHashAndReset();
      }
    }

    private (byte[] hmac, byte[] aes) getKeys(byte[] iv)
    {
      var nonce = iv[0] ^ iv[iv.Length-1];
      var ihmac = nonce % m_HMACKeys.Length;
      var iaes = nonce % m_AESKeys.Length;
      return (m_HMACKeys[ihmac], m_AESKeys[iaes]);
    }

  }
}
