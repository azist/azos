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
  /// Base exception thrown by the Azos Security framework
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
  /// Thrown by Azos security to indicate the authorization problems, such as permission access denial
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
    {
      while(error!=null)
      {
        if (error is AuthorizationException) return true;
        error = error.InnerException;
      }

      return false;
    }

  }
}
