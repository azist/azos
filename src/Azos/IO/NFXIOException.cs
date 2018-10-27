/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.IO
{
  /// <summary>
  /// Base exception thrown by the IO-related classes
  /// </summary>
  [Serializable]
  public class AzosIOException : AzosException
  {
    public AzosIOException() { }
    public AzosIOException(string message) : base(message) { }
    public AzosIOException(string message, Exception inner) : base(message, inner) { }
    protected AzosIOException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}