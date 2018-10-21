using System;
using System.Runtime.Serialization;

namespace Azos.Web.Cloud
{
  /// <summary>
  /// Thrown to indicate workers related problems
  /// </summary>
  [Serializable]
  public class CloudException : WebException
  {
    public CloudException() : base() {}
    public CloudException(string message) : base(message) { }
    public CloudException(string message, Exception inner) : base(message, inner) { }
    protected CloudException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
