/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace Azos.Security
{
  static partial class TheSafe
  {
    ////https://security.stackexchange.com/questions/38828/how-can-i-securely-convert-a-string-password-to-a-key-used-in-aes

    public const string FILE_PREFIX_SAFE = "safe--";
    public const string FILE_EXTENSION_SAFE = ".safe";

    private static readonly Encoding PROTECTION_STRING_ENCODING = new UTF8Encoding(false, false);
    private const int KDF_ITERATIONS = 15_000;

    private const int PREAMBLE_LEN = 0x08;
    private const byte PREAMBLE_0 = 0x2e;
    private const byte PREAMBLE_1 = 0x53;
    private const byte PREAMBLE_2 = 0x41;
    private const byte PREAMBLE_3 = 0x46;
    private const byte PREAMBLE_4 = 0x45;
    private const byte PREAMBLE_5 = 0x20;
    private const byte PREAMBLE_6 = 0x20;
    private const byte PREAMBLE_7 = 0x00;


    private const int PREIV_LEN = 256;
    private const int IV_LEN = 128 / 8; // 16 bytes
    private const int SALT_LEN = 256 / 8; // 32 bytes
    private const int HMAC_LEN = 256 / 8; // 32 bytes

    private const int AES_KEY_LEN = 256 / 8; // 32 bytes
    private const int HMAC_KEY_LEN = 512 / 8; // 64 bytes

    private const int HDR_LEN = PREAMBLE_LEN + PREIV_LEN + HMAC_LEN + HMAC_LEN;

    /// <summary>
    /// Protects a string value with a password
    /// </summary>
    public static byte[] ProtectString(string content, string password)
    {
      content.NonBlank(nameof(content));
      var buf = PROTECTION_STRING_ENCODING.GetBytes(content);
      return Protect(buf, password);
    }

    /// <summary>
    /// Protects byte[] with a password
    /// </summary>
    public static byte[] Protect(byte[] content, string password)
    {
      (content.NonNull(nameof(content)).Length > 0).IsTrue("content.len > 0");
      password.NonBlank(nameof(password));

      var preiv = GenerateRandomBytes(PREIV_LEN);
      var pwdBytes = PROTECTION_STRING_ENCODING.GetBytes(password);
      var pwdFnv = ShardKey.ForBytes(pwdBytes);

      var iv      = stitchPreIvBuffer(preiv, pwdFnv, IV_LEN);
      var kdfSalt = stitchPreIvBuffer(preiv, pwdFnv, SALT_LEN);

      byte[] keyAes;
      byte[] keyPlainHmac;
      byte[] keyCipherHmac;
      using (var kdf = new Rfc2898DeriveBytes(pwdBytes, kdfSalt, KDF_ITERATIONS, HashAlgorithmName.SHA256))
      {
        keyAes = kdf.GetBytes(AES_KEY_LEN);
        keyPlainHmac = kdf.GetBytes(HMAC_KEY_LEN);
        keyCipherHmac = kdf.GetBytes(HMAC_KEY_LEN);
      }

//Azos.Scripting.Conout.SeeArgs("keyAes: \n:{0}", keyAes.ToHexDump());
//Azos.Scripting.Conout.SeeArgs("keyPlainHmac: \n:{0}", keyPlainHmac.ToHexDump());
//Azos.Scripting.Conout.SeeArgs("keyCipherHmac: \n:{0}", keyCipherHmac.ToHexDump());

      Array.Clear(pwdBytes, 0, pwdBytes.Length);

      using(var aes = makeAes())
      {
        using (var encryptor = aes.CreateEncryptor(keyAes, iv))
        {
//Azos.Scripting.Conout.SeeArgs("encrypted: \n:{0}", content.ToHexDump());
          var encrypted = encryptor.TransformFinalBlock(content, 0, content.Length);
          var hmacPlain  = getHMAC(keyPlainHmac,  new ArraySegment<byte>(iv), content);
          var hmacCipher = getHMAC(keyCipherHmac, new ArraySegment<byte>(iv), encrypted);
          Array.Clear(keyAes, 0, keyAes.Length);
          Array.Clear(keyPlainHmac, 0, keyPlainHmac.Length);
          Array.Clear(keyCipherHmac, 0, keyCipherHmac.Length);

          var totalSize = HDR_LEN + encrypted.Length;
          var result = new byte[totalSize];

          result[0] = PREAMBLE_0;
          result[1] = PREAMBLE_1;
          result[2] = PREAMBLE_2;
          result[3] = PREAMBLE_3;
          result[4] = PREAMBLE_4;
          result[5] = PREAMBLE_5;
          result[6] = PREAMBLE_6;
          result[7] = PREAMBLE_7;

          var idxResult = PREAMBLE_LEN;
          Array.Copy(preiv,      0, result, idxResult, PREIV_LEN); idxResult += PREIV_LEN;
          Array.Copy(hmacPlain,  0, result, idxResult, HMAC_LEN);  idxResult += HMAC_LEN;
          Array.Copy(hmacCipher, 0, result, idxResult, HMAC_LEN);  idxResult += HMAC_LEN;
          Array.Copy(encrypted,  0, result, idxResult, encrypted.Length);
          return result;
        }
      }
    }

    /// <summary>
    /// Unprotects original content protected with complementary <see cref="ProtectString(string, string)"/> method.
    /// Returns null if password is wrong or protected content was tampered with
    /// </summary>
    public static string UnprotectString(byte[] content, string password)
    {
      var raw = Unprotect(content, password);
      if (raw == null) return null;
      var result = PROTECTION_STRING_ENCODING.GetString(raw);
      return result;
    }

    /// <summary>
    /// Unprotects original content protected with complementary <see cref="Protect(byte[], string)"/> method.
    /// Returns null if password is wrong or protected content was tampered with
    /// </summary>
    public static byte[] Unprotect(byte[] content, string password)
    {
      (content.NonNull(nameof(content)).Length > HDR_LEN).IsTrue("content.len > hdr");
      password.NonBlank(nameof(password));

      //Invalid content type
      if (!HasPreamble(content, 0)) return null;

      var pwdBytes = PROTECTION_STRING_ENCODING.GetBytes(password);
      var pwdFnv = ShardKey.ForBytes(pwdBytes);

      var preiv = new byte[PREIV_LEN];
      Array.Copy(content, PREAMBLE_LEN, preiv, 0, PREIV_LEN);

      var iv = stitchPreIvBuffer(preiv, pwdFnv, IV_LEN);
      var kdfSalt = stitchPreIvBuffer(preiv, pwdFnv, SALT_LEN);

      byte[] keyAes;
      byte[] keyPlainHmac;
      byte[] keyCipherHmac;
      using (var kdf = new Rfc2898DeriveBytes(pwdBytes, kdfSalt, KDF_ITERATIONS, HashAlgorithmName.SHA256))
      {
        keyAes = kdf.GetBytes(AES_KEY_LEN);
        keyPlainHmac = kdf.GetBytes(HMAC_KEY_LEN);
        keyCipherHmac = kdf.GetBytes(HMAC_KEY_LEN);
      }

//Azos.Scripting.Conout.SeeArgs("keyAes: \n:{0}", keyAes.ToHexDump());
//Azos.Scripting.Conout.SeeArgs("keyPlainHmac: \n:{0}", keyPlainHmac.ToHexDump());
//Azos.Scripting.Conout.SeeArgs("keyCipherHmac: \n:{0}", keyCipherHmac.ToHexDump());

      Array.Clear(pwdBytes, 0, pwdBytes.Length);

      try
      {
        using (var aes = makeAes())
        {
          using (var decryptor = aes.CreateDecryptor(keyAes, iv))
          {
            var gotHmacCipher = new byte[HMAC_LEN];
            Array.Copy(content, PREAMBLE_LEN + PREIV_LEN + HMAC_LEN, gotHmacCipher, 0, HMAC_LEN);
            var hmacCipher = getHMAC(keyCipherHmac, new ArraySegment<byte>(iv), new ArraySegment<byte>(content, HDR_LEN, content.Length - HDR_LEN));
            if (!gotHmacCipher.MemBufferEquals(hmacCipher)) return null;//HMAC mismatch: ciphered message has been tampered with

            var decrypted = decryptor.TransformFinalBlock(content, HDR_LEN, content.Length - HDR_LEN);//never disclose the error reason
//Azos.Scripting.Conout.SeeArgs("decrypted: \n:{0}", decrypted.ToHexDump());
            var gotHmacPlain = new byte[HMAC_LEN];
            Array.Copy(content, PREAMBLE_LEN + PREIV_LEN, gotHmacPlain, 0, HMAC_LEN);
            var hmacPlain = getHMAC(keyPlainHmac, new ArraySegment<byte>(iv), new ArraySegment<byte>(decrypted));
            if (!gotHmacPlain.MemBufferEquals(hmacPlain)) return null;//HMAC mismatch: orig message has been tampered with

            return decrypted;
          }
        }
      }
      catch
      {
        return null;// WARNING!!! DO NOT disclose the reason
      }
      finally
      {
        Array.Clear(keyAes, 0, keyAes.Length);
        Array.Clear(keyPlainHmac, 0, keyPlainHmac.Length);
        Array.Clear(keyCipherHmac, 0, keyCipherHmac.Length);
      }
    }

    /// <summary>
    /// Returns true when the specified msg starts with preamble at the offset
    /// </summary>
    public static bool HasPreamble(byte[] msg, int offset)
    {
      msg.NonNull(nameof(msg));
      if (msg.Length <= PREAMBLE_LEN) return false;

      return (msg[offset + 0] == PREAMBLE_0) &&
          (msg[offset + 1] == PREAMBLE_1) &&
          (msg[offset + 2] == PREAMBLE_2) &&
          (msg[offset + 3] == PREAMBLE_3) &&
          (msg[offset + 4] == PREAMBLE_4) &&
          (msg[offset + 5] == PREAMBLE_5) &&
          (msg[offset + 6] == PREAMBLE_6) &&
          (msg[offset + 7] == PREAMBLE_7);
    }

    /// <summary>
    /// Returns true if the specified file exists and starts with preamble
    /// </summary>
    public static bool HasPreambleInFile(string fn)
    {
      if (fn.IsNullOrWhiteSpace()) return false;
      try
      {
        using var fs = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.Read);
        var buf = new byte[PREAMBLE_LEN];
        for(var i=0; i<PREAMBLE_LEN;)
        {
          var got = fs.Read(buf, i, PREAMBLE_LEN - i);
          if (got == 0) break;
          i+= got;
        }
        return HasPreamble(buf, 0);
      }
      catch//e.g. FileNotFound
      {
        return false;
      }
    }

    /// <summary>
    /// For all files in directory and optionally its subdirectories which match
    /// the specified `filePrefix` (default `safe--`) and are NOT protected - no extension `fileExtension` (default `.safe`)
    /// protects contents of the files by applying the specified password and produces a file with `fileExtension`
    /// optionally deleting the original file
    /// </summary>
    /// <param name="path">Root path where to start</param>
    /// <param name="password">Requires protection password</param>
    /// <param name="filePrefix">The prefix of file search, by default `safe--`</param>
    /// <param name="fileExtension">The extension of protected files, default is `.safe`</param>
    /// <param name="recurse">True to include all subdirectories</param>
    /// <param name="deleteOriginal">True to delete the original file AFTER successful protection</param>
    /// <param name="progress">Optional progress/error callback. In case of error return true to keep on processing, false to break</param>
    public static void ProtectDirectory(string path,
                                        string password,
                                        string filePrefix = FILE_PREFIX_SAFE,
                                        string fileExtension = FILE_EXTENSION_SAFE,
                                        bool recurse = false,
                                        bool deleteOriginal = false,
                                        Func<string, string, Exception, bool> progress = null)
    {
      path.NonBlank(nameof(path));
      password.NonBlank(nameof(password));
      filePrefix.NonBlank(nameof(filePrefix));
      fileExtension = fileExtension.NonBlank(nameof(fileExtension)).Trim();
      fileExtension.StartsWith(".").IsTrue("fileExtension starting with a dot");

      var all = path.AllFileNamesThatMatch(filePrefix + "*", recurse);
      foreach(var fn in all)
      {
        var ext = Path.GetExtension(fn).Trim();
        if (ext.EqualsIgnoreCase(fileExtension)) continue;//filter out already protected files

        var fnSafe = fn + fileExtension;
        if (progress != null)
        {
          progress(fn, fnSafe, null);
        }
        try
        {
          var plain = File.ReadAllBytes(fn);
          if (HasPreamble(plain, 0)) continue;//ensure absence of preamble in file

          var cipher = Protect(plain, password);
          File.WriteAllBytes(fnSafe, cipher);
          if (deleteOriginal) File.Delete(fn);
        }
        catch(Exception cause)
        {
          if (progress != null)
          {
            if (!progress(fn, fnSafe, cause)) throw;
          }
          else throw;
        }
      }//foreach
    }


    /// <summary>
    /// For all files in directory and optionally its subdirectories which match
    /// the specified `filePrefix` (default `safe--`) and are protected - have extension `fileExtension` (default `.safe`)
    /// unprotects contents of the files by applying the specified password and produces a file without `fileExtension`
    /// optionally deleting the original protected file
    /// </summary>
    /// <param name="path">Root path where to start</param>
    /// <param name="password">Requires protection password</param>
    /// <param name="filePrefix">The prefix of file search, by default `safe--`</param>
    /// <param name="fileExtension">The extension of protected files, default is `.safe`</param>
    /// <param name="recurse">True to include all subdirectories</param>
    /// <param name="deleteOriginal">True to delete the original file AFTER successful protection</param>
    /// <param name="progress">Optional progress/error callback. In case of error return true to keep on processing, false to break</param>
    public static void UnprotectDirectory(string path,
                                        string password,
                                        string filePrefix = FILE_PREFIX_SAFE,
                                        string fileExtension = FILE_EXTENSION_SAFE,
                                        bool recurse = false,
                                        bool deleteOriginal = false,
                                        Func<string, string, Exception, bool> progress = null)
    {
      path.NonBlank(nameof(path));
      password.NonBlank(nameof(password));
      filePrefix.NonBlank(nameof(filePrefix));
      fileExtension = fileExtension.NonBlank(nameof(fileExtension)).Trim();
      fileExtension.StartsWith(".").IsTrue("fileExtension starting with a dot");

      var all = path.AllFileNamesThatMatch(filePrefix + "*", recurse);
      foreach (var fn in all)
      {
        var ext = Path.GetExtension(fn).Trim();
        if (!ext.EqualsIgnoreCase(fileExtension)) continue;//filter out already unprotected files


        var fnOriginal = Path.Join(Path.GetDirectoryName(fn), Path.GetFileNameWithoutExtension(fn));
        if (progress != null)
        {
          progress(fn, fnOriginal, null);
        }
        try
        {
          var cipher = File.ReadAllBytes(fn);
          if (!HasPreamble(cipher, 0)) continue;//ensure preamble in file

          var original = Unprotect(cipher, password);
          if (original == null) throw new SecurityException("Could not unprotect file: `{0}`".Args(fn));
          File.WriteAllBytes(fnOriginal, original);
          if (deleteOriginal) File.Delete(fn);
        }
        catch (Exception cause)
        {
          if (progress != null)
          {
            if (!progress(fn, fnOriginal, cause)) throw;
          }
          else throw;
        }
      }//foreach
    }

    private static byte[] stitchPreIvBuffer(byte[] preiv, ulong secret, int blen)
    {
      (blen <= preiv.Length).IsTrue("blen <= preiv.Length");
      var result = new byte[blen];
      var slice = blen / sizeof(ulong);
      for(int i=0, j=0; i < result.Length; i++, j++)
      {
        if (secret != 0 && i % slice == 0)
        {
          j = (int)(secret & 0xff);
          secret >>= 8;
        }

        if (j >= preiv.Length) j = 0;

        result[i] = preiv[j];
      }

      return result;
    }

    private static AesManaged makeAes()
    {
      var aes = new AesManaged();
      aes.Mode = CipherMode.CBC;//Cipher Block Chaining requires random 128bit IV
      aes.KeySize = 256;
      aes.Padding = PaddingMode.PKCS7;
      return aes;
    }

    private static byte[] getHMAC(byte[] keyHMAC, ArraySegment<byte> nonce, ArraySegment<byte> data)
    {
      using (var ihash = IncrementalHash.CreateHMAC(HashAlgorithmName.SHA256, keyHMAC))
      {
        ihash.AppendData(nonce.Array, nonce.Offset, nonce.Count);
        ihash.AppendData(data.Array, data.Offset, data.Count);
        return ihash.GetHashAndReset();
      }
    }
  }
}
