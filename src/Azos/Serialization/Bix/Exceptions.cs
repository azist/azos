/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Serialization.Bix
{
  /// <summary>
  /// Base exception thrown by the Bix serialization framework
  /// </summary>
  [Serializable]
  public class BixException : AzosSerializationException
  {
    public BixException() { }
    public BixException(string message) : base(message) { }
    public BixException(string message, Exception inner) : base(message, inner) { }
    protected BixException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}