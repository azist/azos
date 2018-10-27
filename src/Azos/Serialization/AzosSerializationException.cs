/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Azos.Serialization
{
  /// <summary>
  /// Base exception thrown by the serialization-related classes
  /// </summary>
  [Serializable]
  public class AzosSerializationException : AzosException
  {
    public AzosSerializationException() { }
    public AzosSerializationException(string message) : base(message) { }
    public AzosSerializationException(string message, Exception inner) : base(message, inner) { }
    protected AzosSerializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}