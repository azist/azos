/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Log
{
  /// <summary>
  /// Base exception thrown by log-related framework
  /// </summary>
  [Serializable]
  public class LogException : AzosException
  {
    public LogException() { }
    public LogException(string message) : base(message) { }
    public LogException(string message, Exception inner) : base(message, inner) { }
    protected LogException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}