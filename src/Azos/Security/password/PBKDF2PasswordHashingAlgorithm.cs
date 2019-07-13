/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Security.Cryptography;

using Azos.Data;
using Azos.Platform.Abstraction;

namespace Azos.Security
{
  public struct PBKDF2PasswordHashingOptions : IPasswordHashingOptions
  {
    public byte[] Salt { get; set; }
  }

  /// <summary>
  /// Implements Password KEY Derivation function based on Rfc2898DeriveBytes/HMACSHA256
  /// See: https://www.owasp.org/index.php/Using_Rfc2898DeriveBytes_for_PBKDF2 , https://crackstation.net/hashing-security.htm
  /// </summary>
  public sealed class PBKDF2PasswordHashingAlgorithm : PasswordHashingAlgorithm<PBKDF2PasswordHashingOptions>
  {
    public const int SALT_LENGTH_BYTES = 32;
    public const int HASH_LENGTH_BYTES = 32;//HMAC-SHA256 = 256/8 = 32

    public PBKDF2PasswordHashingAlgorithm(IPasswordManagerImplementation director, string name) : base(director, name)
    {
    }

    private int getIterations()
    {
      switch (StrengthLevel)
      {
        case PasswordStrengthLevel.Minimum: return 1_000;
        case PasswordStrengthLevel.BelowNormal: return 5_000;
        case PasswordStrengthLevel.AboveNormal: return 25_000;
        case PasswordStrengthLevel.Maximum: return 75_000;
        default: return 12_000;
      }
    }

    protected override HashedPassword DoComputeHash(PasswordFamily family, SecureBuffer password, PBKDF2PasswordHashingOptions options)
    {
      var salt = options.Salt;
      var content = password.Content;

      var iterations = getIterations();

      //https://stackoverflow.com/questions/18648084/rfc2898-pbkdf2-with-sha256-as-digest-in-c-sharp
      var hash = PlatformAbstractionLayer.Cryptography.ComputePBKDF2(content, salt, HASH_LENGTH_BYTES, iterations, HashAlgorithmName.SHA256);

      var pwd = new HashedPassword(Name, family)
      {
        { "h", hash.ToWebSafeBase64() },
        { "s", salt.ToWebSafeBase64() }
      };

      Array.Clear(hash, 0, hash.Length);

      return pwd;
    }

    protected override PBKDF2PasswordHashingOptions DefaultPasswordHashingOptions
     => new PBKDF2PasswordHashingOptions
     {
        Salt = App.SecurityManager.Cryptography.GenerateRandomBytes(SALT_LENGTH_BYTES)
     };

    protected override PBKDF2PasswordHashingOptions DoExtractPasswordHashingOptions(HashedPassword hash, out bool needRehash)
    {
      needRehash = false;
      return new PBKDF2PasswordHashingOptions
      {
        Salt = hash["s"].AsString().FromWebSafeBase64()
      };
    }

    protected override bool DoAreEquivalent(HashedPassword hash, HashedPassword rehash)
    {
      var a = hash["h"].AsString();
      var b = rehash["h"].AsString();
      return HashedPassword.AreStringsEqualInLengthConstantTime(a, b);
    }
  }
}
