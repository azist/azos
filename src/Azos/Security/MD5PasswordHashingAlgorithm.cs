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

  public class MD5PasswordHashingAlgorithm : PasswordHashingAlgorithm<MD5PasswordHashingOptions>
  {
    public const int DEFAULT_SALT_MAX_LENGTH = 32;

    public MD5PasswordHashingAlgorithm(IPasswordManagerImplementation director, string name) : base(director, name)
    {
      SaltMinLenght = DEFAULT_SALT_MAX_LENGTH / 2;
      SaltMaxLenght = DEFAULT_SALT_MAX_LENGTH;
    }

    #region Properties

      [Config(Default = DEFAULT_SALT_MAX_LENGTH / 2)]
      public int SaltMinLenght { get; set; }

      [Config(Default = DEFAULT_SALT_MAX_LENGTH)]
      public int SaltMaxLenght { get; set; }

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
    {
      get
      {
        return new MD5PasswordHashingOptions
        {
          Salt = App.Random.NextRandomBytes(SaltMinLenght, SaltMaxLenght)
        };
      }
    }

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
      return hash["hash"].AsString().EqualsOrdIgnoreCase(rehash["hash"].AsString());
    }
  }
}
