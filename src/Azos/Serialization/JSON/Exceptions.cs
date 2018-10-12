
using System;
using System.Runtime.Serialization;

namespace Azos.Serialization.JSON
{
  /// <summary>
  /// Base exception thrown by the JSON serialization format
  /// </summary>
  [Serializable]
  public class JSONException : AzosSerializationException
  {
    public JSONException() { }
    public JSONException(string message) : base(message) { }
    public JSONException(string message, Exception inner) : base(message, inner) { }
    protected JSONException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Base exception thrown by the JSON when serializing objects
  /// </summary>
  [Serializable]
  public class JSONSerializationException : JSONException
  {
    public JSONSerializationException() { }
    public JSONSerializationException(string message) : base(message) { }
    public JSONSerializationException(string message, Exception inner) : base(message, inner) { }
    protected JSONSerializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Base exception thrown by the JSON when deserializing objects
  /// </summary>
  [Serializable]
  public class JSONDeserializationException : JSONException
  {
    public JSONDeserializationException() { }
    public JSONDeserializationException(string message) : base(message) { }
    public JSONDeserializationException(string message, Exception inner) : base(message, inner) { }
    protected JSONDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}