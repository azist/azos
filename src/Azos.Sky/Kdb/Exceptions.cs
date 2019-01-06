/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Kdb
{
  /// <summary>
  /// Thrown to indicate KDB-related problems
  /// </summary>
  [Serializable]
  public class KdbException : SkyException
  {
    public KdbException() : base() { }
    public KdbException(string message) : base(message) { }
    public KdbException(string message, Exception inner) : base(message, inner) { }
    protected KdbException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
