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
  /// Defines password stregth levels: Minimum, Normal, Maximum etc.
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
  /// Denoutes kinds of passwords i.e.: text that user types on login, short PIN,
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
  /// Denotes an entity that manages passwords such as: computes and verified hash tokens
  /// and provides password strength verification
  /// </summary>
  public interface IPasswordManager : IApplicationComponent
  {
    HashedPassword ComputeHash(PasswordFamily family, SecureBuffer password, PasswordStrengthLevel level = PasswordStrengthLevel.Default);
    bool Verify(SecureBuffer password, HashedPassword hash, out bool needRehash);
    bool AreEquivalent(HashedPassword a, HashedPassword b);

    int CalculateStrenghtScore(PasswordFamily family, SecureBuffer password);
    int CalculateStrenghtPercent(PasswordFamily family, SecureBuffer password, int maxScore = 0);
    IEnumerable<PasswordRepresentation> GeneratePassword(PasswordFamily family, PasswordRepresentationType type, PasswordStrengthLevel level = PasswordStrengthLevel.Default);

    Collections.IRegistry<PasswordHashingAlgorithm> Algorithms { get; }
  }

  public interface IPasswordManagerImplementation : IPasswordManager, IDisposable, IConfigurable, IInstrumentable, IDaemon
  {
    bool Register(PasswordHashingAlgorithm algo);
    bool Unregister(PasswordHashingAlgorithm algo);
    bool Unregister(string algoName);
  }
}

