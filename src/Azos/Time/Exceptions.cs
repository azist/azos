/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Time
{
  /// <summary>
  /// Base exception thrown by the Time framework
  /// </summary>
  [Serializable]
  public class TimeException : AzosException
  {
    public TimeException() { }
    public TimeException(string message) : base(message) { }
    public TimeException(string message, Exception inner) : base(message, inner) { }
    protected TimeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
