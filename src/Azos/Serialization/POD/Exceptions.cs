/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Azos.Serialization.POD
{
  /// <summary>
  /// Base exception thrown by the Portable Object Document serialization format
  /// </summary>
  [Serializable]
  public class PODException : AzosSerializationException
  {
    public PODException() { }
    public PODException(string message) : base(message) { }
    public PODException(string message, Exception inner) : base(message, inner) { }
    protected PODException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Base exception thrown by the PortableObjectDocument when serializing objects into document
  /// </summary>
  [Serializable]
  public class PODSerializationException : PODException
  {
    public PODSerializationException() { }
    public PODSerializationException(string message) : base(message) { }
    public PODSerializationException(string message, Exception inner) : base(message, inner) { }
    protected PODSerializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Base exception thrown by the PortableObjectDocument when deserializing original objects
  /// </summary>
  [Serializable]
  public class PODDeserializationException : PODException
  {
    public PODDeserializationException() { }
    public PODDeserializationException(string message) : base(message) { }
    public PODDeserializationException(string message, Exception inner) : base(message, inner) { }
    protected PODDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}