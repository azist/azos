/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Security
{
  /// <summary>
  /// Represents a named algorithm instance of a specific type along with its keys/init
  /// vectors and other config which is specific to this algorithm
  /// </summary>
  public interface ICryptoMessageAlgorithm : IApplicationComponent, INamed
  {
    /// <summary>
    /// Protects the specified message according to the underlying algorithm nature, e.g. a hashing algorithm may
    /// just add a HMAC, or encrypt the message body (e.g. with AES) etc. using same or different key(s) for various stages.
    /// The message must be processed with complementary call to Unprotect() which understands the inner msg format
    /// </summary>
    byte[] Protect(ArraySegment<byte> originalMessage);

    /// <summary>
    /// A complementary method for Protect(), tries to read the supplied protected message content, peforming necessary
    /// transforms, such as encryption/decryption, HMAC checking etc.. The details are up to a specific algorithm type and instance configuration.
    /// If message is not authentic/tempered then NULL is returned
    /// </summary>
    /// <param name="protectedMessage">Data to decrypt</param>
    /// <returns>Original message data if all checks pass, or null if message is corrupted/has been tempered with</returns>
    byte[] Unprotect(ArraySegment<byte> protectedMessage);
  }

  public interface ICryptoAlgorithmImplementation : ICryptoMessageAlgorithm, IDisposable, IConfigurable, IDaemon
  {

  }

  /// <summary>
  /// Provides cryptography services, such as encryption/decryption/verification of content
  /// </summary>
  public interface ICryptoManager : IApplicationComponent
  {
    IRegistry<ICryptoMessageAlgorithm> MessageAlgorithms { get; }
  }

  public interface ICryptoManagerImplementation : ICryptoManager, IDisposable, IConfigurable, IInstrumentable, IDaemon
  {
    bool Register(ICryptoAlgorithmImplementation algo);
    bool Unregister(ICryptoAlgorithmImplementation algo);
    bool Unregister(string algoName);
  }


}

