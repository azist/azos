/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Sky.WebMessaging
{
  /// <summary>
  /// Thrown to indicate problems related to web messaging
  /// </summary>
  [Serializable]
  public class WebMessagingException : SkyException
  {
    public WebMessagingException() : base() {}
    public WebMessagingException(string message) : base(message) {}
    public WebMessagingException(string message, Exception inner) : base(message, inner) { }
    protected WebMessagingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
