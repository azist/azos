/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Security
{
  /// <summary>
  /// Defines password strength levels: Minimum, Normal, Maximum etc.
  /// </summary>
  public enum PasswordStrengthLevel
  {
    Default = 0,
    Minimum,
    BelowNormal,
    Normal,
    AboveNormal,
    Maximum
  }

  /// <summary>
  /// Denotes kinds of passwords i.e.: text that user types on login, short PIN,
  /// geometrical curve that users need to trace with their finger, select areas of picture
  /// </summary>
  public enum PasswordFamily
  {
    Unspecified,
    Text,
    PIN,
    Geometry,
    Picture,
    Other
  }


  /// <summary>
  /// Sets contract for an entity that manages passwords such as: computes and verified hash tokens,
  /// and provides password strength verification
  /// </summary>
  public interface IPasswordManager : IApplicationComponent
  {
    /// <summary>
    /// Computes a hashed version of the password supplied as SecureBuffer
    /// </summary>
    HashedPassword ComputeHash(PasswordFamily family, SecureBuffer password, PasswordStrengthLevel level = PasswordStrengthLevel.Default);

    /// <summary>
    /// Verifies the password, returning true on success
    /// </summary>
    /// <param name="password">SecureBuffer filled with password representation which is being checked</param>
    /// <param name="hash">The hashed password which is stored in the credentials store</param>
    /// <param name="needRehash">
    /// Returns true when the supplied hash does not satisfy the current password policy and the password needs to be re-hashed, for example
    /// you may have used an older hashing algorithm and now upgraded to more secure algorithm or changed algorithm parameter (such as iteration count)
    /// </param>
    /// <returns>True if password matches the hashed password using the algorithm instance specified in hash</returns>
    bool Verify(SecureBuffer password, HashedPassword hash, out bool needRehash);

    /// <summary>
    ///  Returns true two passwords are the same: use the same algorithm and the same hash
    /// </summary>
    bool AreEquivalent(HashedPassword a, HashedPassword b);

    /// <summary>
    /// Calculates strength score which is not normalized (can exceed 100)
    /// </summary>
    int CalculateStrenghtScore(PasswordFamily family, SecureBuffer password);

    /// <summary>
    /// Calculates password strength score normalized to top 100
    /// </summary>
    int CalculateStrenghtPercent(PasswordFamily family, SecureBuffer password, int maxScore = 0);

    /// <summary>
    /// Generates new password
    /// </summary>
    IEnumerable<PasswordRepresentation> GeneratePassword(PasswordFamily family,
                                                         PasswordRepresentationType type,
                                                         PasswordStrengthLevel level = PasswordStrengthLevel.Default);

    /// <summary>
    /// A registry of PasswordHashingAlgorithm
    /// </summary>
    Collections.IRegistry<PasswordHashingAlgorithm> Algorithms { get; }
  }

  public interface IPasswordManagerImplementation : IPasswordManager, IDisposable, IConfigurable, IInstrumentable, IDaemon
  {
    bool Register(PasswordHashingAlgorithm algo);
    bool Unregister(PasswordHashingAlgorithm algo);
    bool Unregister(string algoName);
  }
}

