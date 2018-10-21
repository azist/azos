
using System;
using System.Runtime.Serialization;

namespace Azos.Web.GeoLookup
{
  /// <summary>
  /// Base exception class thrown by geo-related logic
  /// </summary>
  [Serializable]
  public class GeoException : WebException
  {
    public GeoException() { }
    public GeoException(string message) : base(message) { }
    public GeoException(string message, Exception inner) : base(message, inner) { }
    protected GeoException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
