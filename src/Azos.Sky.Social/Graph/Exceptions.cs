using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Social
{
  /// <summary>
  /// Base exception thrown by the social Graph framework
  /// </summary>
  [Serializable]
  public class GraphException : SocialException
  {
    public GraphException() : base() { }
    public GraphException(int code) : base(code) { }
    public GraphException(int code, string message) : base(code, message) { }
    public GraphException(string message) : base(message) { }
    public GraphException(string message, Exception inner) : base(message, inner) { }
    public GraphException(string message, Exception inner, int code, string sender, string topic) : base(message, inner, code, sender, topic) { }
    protected GraphException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
