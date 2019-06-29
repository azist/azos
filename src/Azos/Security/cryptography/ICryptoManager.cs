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
  /// Designates types of message protection algorithm, such as Cipher/Decipher vs MAC.
  /// The MAC algorithms typically add the MAC code to the message body
  /// </summary>
  [Flags]
  public enum CryptoMessageAlgorithmFlags
  {
    /// <summary>
    /// The algorithm is Cipher/Decipher used for message passing
    /// </summary>
    Cipher = 1,

    /// <summary>
    /// The algorithm is Message Authenticity Code (e.g. HMACSHA256)
    /// </summary>
    MAC = 1 << 1,

    /// <summary>
    /// When set indicates that algorithm supports Unprotect() calls.
    /// Not all algorithms do, for example HMACSHA256 algorithm just generates a separate code without embedding it into message.
    /// You would need to call Protect() on the receiving side and check the result and communicate the MAC in some other way.
    /// </summary>
    CanUnprotect = 1 << 2,
  }

  /// <summary>
  /// Denotes types of users should be using this algorithm , e.g. a security authority may use its own Internal algorithm instances
  /// (i.e. different encryption keys) that should never be used for public message authentication
  /// </summary>
  public enum CryptoMessageAlgorithmAudience
  {
    /// <summary>
    /// Denotes algorithms instances that should be used for public message exchange,
    /// for example messages which roundtrip through public clients like browsers/mobile apps etc.
    /// </summary>
    Public,

    /// <summary>
    /// Denotes algorithms instances that should be used for internal in-system message exchange,
    /// for example messages which roundtrip between cluster peers, such as the ones used for AuthenticationToken issuance.
    /// Typically this instance uses a system-private key only known inside of the authentication authority perimeter
    /// </summary>
    Internal
  }

  /// <summary>
  /// Represents a named algorithm instance of a specific type along with its keys/init
  /// vectors and other config which is specific to this algorithm
  /// </summary>
  public interface ICryptoMessageAlgorithm : IApplicationComponent, INamed
  {
    /// <summary>
    /// Defines what algorithm type this instance represents
    /// </summary>
    CryptoMessageAlgorithmFlags Flags { get;}

    /// <summary>
    /// Indicates what audience should be using this algorithm
    /// </summary>
    CryptoMessageAlgorithmAudience Audience {  get; }

    /// <summary>
    /// True to indicate that this algorithm instance shall be used over others matching algorithms
    /// </summary>
    bool IsDefault{ get;}

    /// <summary>
    /// Protects the specified message according to the underlying algorithm nature, e.g. a hashing MAC algorithm may
    /// just add a HMAC, or encrypt the message body (e.g. with AES) etc. using same or different key(s) for various stages.
    /// An algorithm may by symmetric or asymmetric. For algorithms that have Flags.CanUnprotect set, the message must
    /// be processed with complementary call to Unprotect() which understands the inner msg format
    /// </summary>
    byte[] Protect(ArraySegment<byte> originalMessage);

    /// <summary>
    /// A complementary method for Protect(), tries to read the supplied protected message content, performing necessary
    /// transforms, such as encryption/decryption, HMAC checking etc.. The details are up to a specific algorithm type and instance configuration.
    /// If message is not authentic/tempered then NULL is returned.
    /// This method only runs if algorithms supports it, i.e.  Flags.CanUnprotect set to 1.
    /// </summary>
    /// <param name="protectedMessage">Data to decrypt</param>
    /// <returns>Original message data if all checks pass, or null if message is corrupted/has been tempered with</returns>
    byte[] Unprotect(ArraySegment<byte> protectedMessage);
  }

  public interface ICryptoMessageAlgorithmImplementation : ICryptoMessageAlgorithm, IDisposable, IConfigurable, IDaemon
  {
  }

  /// <summary>
  /// Provides cryptography services, such as encryption/decryption/verification of messages
  /// </summary>
  public interface ICryptoManager : IApplicationComponent
  {
    /// <summary>
    /// A registry of algorithms which cryptographically protect messages
    /// </summary>
    IRegistry<ICryptoMessageAlgorithm> MessageProtectionAlgorithms { get; }
  }

  public interface ICryptoManagerImplementation : ICryptoManager, IDisposable, IConfigurable, IInstrumentable, IDaemon
  {
  }


}

