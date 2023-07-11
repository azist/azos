/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Security
{
  /// <summary>
  /// Base exception thrown by the Azos Security framework.
  /// This exception does not mean that access was denied, rather it implies security-related error.
  /// Contrast with AuthorizationException which specifically indicates violation of access/insufficient rights.
  /// </summary>
  [Serializable]
  public class SecurityException : AzosException, ISecurityException
  {
    public SecurityException() { }
    public SecurityException(string message) : base(message) { }
    public SecurityException(string message, Exception inner) : base(message, inner) { }
    protected SecurityException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// A sub type of <see cref="SecurityException"/>
  /// which denotes an invalid denied access attempt to the <see cref="TheSafe"/>.
  /// This condition indicates either an invalid access attempt or implementation error
  /// where the caller forgot to set the security checks with <see cref="SecurityFlowScope"/>
  /// proper Safe flag inclusion that would have signified the intent to use <see cref="TheSafe"/>
  /// </summary>
  [Serializable]
  public sealed class AccessToTheSafeDeniedException : SecurityException
  {
    public AccessToTheSafeDeniedException() { }
    public AccessToTheSafeDeniedException(string message) : base(message) { }
    public AccessToTheSafeDeniedException(string message, Exception inner) : base(message, inner) { }
    private AccessToTheSafeDeniedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by Azos security framework to indicate the authorization problem conditions, such as permission access denial.
  /// This exception is a subtype of a broader SecurityException which denotes problems related to security in general
  /// </summary>
  [Serializable]
  public class AuthorizationException : SecurityException, IHttpStatusProvider
  {
    public AuthorizationException() { }
    public AuthorizationException(string message) : base(message) { }
    public AuthorizationException(string message, Exception inner) : base(message, inner) { }
    protected AuthorizationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public int    HttpStatusCode => WebConsts.STATUS_403;
    public string HttpStatusDescription => WebConsts.STATUS_403_DESCRIPTION;


    /// <summary>
    /// Returns true when the specified exception is AuthorizationException or AuthorizationException exists
    /// in the chain of InnerException/s of the specified exception
    /// </summary>
    public static bool IsDenotedBy(Exception error)
     => error.SearchThisOrInnerExceptionOf<AuthorizationException>() != null;
  }
}
