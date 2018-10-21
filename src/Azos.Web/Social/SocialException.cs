
using System;
using System.Runtime.Serialization;

namespace Azos.Web.Social
{
  /// <summary>
  /// General Azos social network specific exception
  /// </summary>
  [Serializable]
  public class SocialException: WebException
  {
    public SocialException() { }
    public SocialException(string message) : base(message) { }
    public SocialException(string message, Exception inner) : base(message, inner) { }
    protected SocialException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
