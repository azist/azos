using System;
using System.Runtime.Serialization;

namespace Azos.Web
{
  /// <summary>
  /// Base exception class thrown by Azos.Web assembly
  /// </summary>
  [Serializable]
  public class WebException : AzosException
  {
    public WebException() { }
    public WebException(string message) : base(message) { }
    public WebException(string message, Exception inner) : base(message, inner) { }
    protected WebException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
