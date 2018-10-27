/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Serialization.Arow
{
  /// <summary>
  /// Base exception thrown by the Arow serialization format
  /// </summary>
  [Serializable]
  public class ArowException : AzosSerializationException
  {
    public ArowException() { }
    public ArowException(string message) : base(message) { }
    public ArowException(string message, Exception inner) : base(message, inner) { }
    protected ArowException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}