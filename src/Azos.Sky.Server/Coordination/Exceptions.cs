/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Coordination
{
  /// <summary>
  /// Thrown to indicate coordination-related problems
  /// </summary>
  [Serializable]
  public class CoordinationException : SkyException
  {
    public CoordinationException() : base() { }
    public CoordinationException(string message) : base(message) { }
    public CoordinationException(string message, Exception inner) : base(message, inner) { }
    protected CoordinationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
