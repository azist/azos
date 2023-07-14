/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;

namespace Azos.Security
{
  /// <summary>
  /// Provides a simple high level API for protecting content, such as byte[] using just a string password
  /// with related key derivation function used for HMAC check and AES encryption
  /// </summary>
  public static class PasswordProtectedContent
  {
    //https://security.stackexchange.com/questions/38828/how-can-i-securely-convert-a-string-password-to-a-key-used-in-aes

    private static readonly System.Text.Encoding STRING_HASHING_ENCODING = new System.Text.UTF8Encoding(false, false);


    public static byte[] ProtectString(string content, string password)
    {
     var buf = STRING_HASHING_ENCODING.GetBytes(content);
     return Protect(buf, password);
    }

    public static byte[] Protect(byte[] content, string password)
    {
      var salt = new byte[16];//generate salt using RNG
      //var iv = generate RNG IV
      //generate key from PbkDeriveBytes into AES
      //us HMAC for content
      //write all in one array

      //using (var kdf = new Rfc2898DeriveBytes(buf, STRING_HASHING_SALT_32, 600_000, HashAlgorithmName.SHA256))
      //{
      //  return kdf.GetBytes(32);//bytes
      //}

      return null;
    }

    public static byte[] Unprotect(byte[] content, string password)
    {
      return null;
    }

    public static string UnprotectString(byte[] content, string password)
    {
      return null;
    }

  }
}
