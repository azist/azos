/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
  public class JSONDeserializationException : JSONException, IExternalStatusProvider
  {
    public JSONDeserializationException() { }
    public JSONDeserializationException(string message) : base(message) { }
    public JSONDeserializationException(string message, Exception inner) : base(message, inner) { }
    protected JSONDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public virtual JsonDataMap ProvideExternalStatus(bool includeDump)
    {
      return this.DefaultBuildErrorStatusProviderMap(includeDump, "ser.json");
    }
  }
}