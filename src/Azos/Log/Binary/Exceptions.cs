/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Log.Binary
{
  /// <summary>
  /// Base exception thrown by the binlog-related framework
  /// </summary>
  [Serializable]
  public class BinLogException : AzosException
  {
    public BinLogException() { }
    public BinLogException(string message) : base(message) { }
    public BinLogException(string message, Exception inner) : base(message, inner) { }
    protected BinLogException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}