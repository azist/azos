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
  /// <summary>
  /// Implements NOP password hashing which does not perform extra hashing and compares verbatim passwords strings char-by-char sensing case.
  /// This algorithm is typically used with hard-coded API tokens in testing scenarios.
  /// It should not be used in production systems as production should use real API keys, not hard-coded ones used for testing
  /// </summary>
  public sealed class NopPasswordHashingAlgorithm : PasswordHashingAlgorithm<IPasswordHashingOptions>
  {

    public NopPasswordHashingAlgorithm(IPasswordManagerImplementation director, string name) : base(director, name)
    {
    }


    protected override HashedPassword DoComputeHash(PasswordFamily family, SecureBuffer password, IPasswordHashingOptions options)
    {
      return new HashedPassword(Name, family)
      {
        { "h", Convert.ToBase64String(password.Content) }
      };
    }

    protected override IPasswordHashingOptions DefaultPasswordHashingOptions => null;

    protected override IPasswordHashingOptions DoExtractPasswordHashingOptions(HashedPassword hash, out bool needRehash)
    {
      needRehash = false;
      return null;
    }

    protected override bool DoAreEquivalent(HashedPassword hash, HashedPassword rehash)
    {
      var a = hash["h"].AsString();
      var b = rehash["h"].AsString();
      return HashedPassword.AreStringsEqualInLengthConstantTime(a, b);
    }
  }
}
