/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.IO.Sipc
{
  /// <summary>
  /// Base exception thrown by the Sipc-related classes
  /// </summary>
  [Serializable]
  public class SipcException : AzosIOException
  {
    public SipcException() { }
    public SipcException(string message) : base(message) { }
    public SipcException(string message, Exception inner) : base(message, inner) { }
    protected SipcException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}