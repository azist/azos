/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;

using Azos.Conf;
using Azos.Data;

namespace Azos.Security
{
  public struct MD5PasswordHashingOptions : IPasswordHashingOptions
  {
    public byte[] Salt { get; set; }
  }

  /// <summary>
  /// Implements MD5 password hashing. This algorithm is provided only for testing and backward compatibility with legacy systems
  /// as MD5 is a weak hash function. Use PBKDF2 for all current development
  /// </summary>
  public class MD5PasswordHashingAlgorithm : PasswordHashingAlgorithm<MD5PasswordHashingOptions>
  {
    public const int DEFAULT_SALT_MAX_LENGTH = 32;

    public MD5PasswordHashingAlgorithm(IPasswordManagerImplementation director, string name) : base(director, name)
    {
      SaltMinLength = DEFAULT_SALT_MAX_LENGTH / 2;
      SaltMaxLength = DEFAULT_SALT_MAX_LENGTH;
    }

    #region Properties

      [Config(Default = DEFAULT_SALT_MAX_LENGTH / 2)]
      public int SaltMinLength { get; set; }

      [Config(Default = DEFAULT_SALT_MAX_LENGTH)]
      public int SaltMaxLength { get; set; }

    #endregion

    protected override HashedPassword DoComputeHash(PasswordFamily family, SecureBuffer password, MD5PasswordHashingOptions options)
    {
      using (var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider())
      {
        var content = password.Content;
        var contentLength = content.Length;
        var salt = options.Salt;
        var buffer = new byte[contentLength + salt.Length];
        Array.Copy(content, buffer, contentLength);
        Array.Copy(salt, 0, buffer, contentLength, salt.Length);
        var hash = md5.ComputeHash(buffer);
        Array.Clear(buffer, 0, buffer.Length);

        return new HashedPassword(Name, family)
        {
          { "hash", Convert.ToBase64String(hash) },
          { "salt", Convert.ToBase64String(salt) }
        };
      }
    }

    protected override MD5PasswordHashingOptions DefaultPasswordHashingOptions
    => new MD5PasswordHashingOptions
       {
         Salt = App.Random.NextRandomBytes(SaltMinLength, SaltMaxLength)
       };

    protected override MD5PasswordHashingOptions DoExtractPasswordHashingOptions(HashedPassword hash, out bool needRehash)
    {
      needRehash = false;
      return new MD5PasswordHashingOptions
      {
        Salt = Convert.FromBase64String(hash["salt"].AsString())
      };
    }

    protected override bool DoAreEquivalent(HashedPassword hash, HashedPassword rehash)
    {
      var a = hash["hash"].AsString();
      var b = rehash["hash"].AsString();
      return HashedPassword.AreStringsEqualInLengthConstantTime(a, b);
    }
  }
}
