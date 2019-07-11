/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Security.Cryptography;
using Azos.Conf;
using Azos.Data;

namespace Azos.Security
{
  public struct PBKDF2PasswordHashingOptions : IPasswordHashingOptions
  {
    public byte[] Salt { get; set; }
  }

  /// <summary>
  /// Implements Password KEy Derivation function based on Rfc2898DeriveBytes/HMACSHA1 Class
  /// See: https://www.owasp.org/index.php/Using_Rfc2898DeriveBytes_for_PBKDF2
  /// </summary>
  public sealed class PBKDF2PasswordHashingAlgorithm : PasswordHashingAlgorithm<PBKDF2PasswordHashingOptions>
  {
    public const int SALT_LENGTH_BYTES = 16;
    public const int HASH_LENGTH_BYTES = 20;//HMAC-SHA1 as per Rfc2898DeriveBytes

    public PBKDF2PasswordHashingAlgorithm(IPasswordManagerImplementation director, string name) : base(director, name)
    {
    }

    private int getIterations()
    {
      switch (StrengthLevel)
      {
        case PasswordStrengthLevel.Minimum: return 1_000;
        case PasswordStrengthLevel.BelowNormal: return 5_000;
        case PasswordStrengthLevel.AboveNormal: return 20_000;
        case PasswordStrengthLevel.Maximum: return 128_000;
        default: return 10_000;
      }
    }

    protected override HashedPassword DoComputeHash(PasswordFamily family, SecureBuffer password, PBKDF2PasswordHashingOptions options)
    {
      var salt = options.Salt;
      var content = password.Content;

      var iterations = getIterations();

      //todo: Move to PAL!!!!
      //Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256)

    //https://stackoverflow.com/questions/18648084/rfc2898-pbkdf2-with-sha256-as-digest-in-c-sharp


      using (var pbkdf2 = new Rfc2898DeriveBytes(content, salt, iterations))
      {
        var hash = pbkdf2.GetBytes(HASH_LENGTH_BYTES);

        return new HashedPassword(Name, family)
        {
          { "h", hash.ToWebSafeBase64() },
          { "s", salt.ToWebSafeBase64() }
        };
      }
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
