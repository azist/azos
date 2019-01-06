/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Mdb
{
  /// <summary>
  /// Thrown to indicate Mdb-related problems
  /// </summary>
  [Serializable]
  public class MdbException : SkyException
  {
    public MdbException() : base() { }
    public MdbException(string message) : base(message) { }
    public MdbException(string message, Exception inner) : base(message, inner) { }
    protected MdbException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
