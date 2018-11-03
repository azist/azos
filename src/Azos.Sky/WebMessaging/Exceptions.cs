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
