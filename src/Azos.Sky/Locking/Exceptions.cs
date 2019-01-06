/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Locking
{
  /// <summary>
  /// Thrown to indicate problems related to distributed locking/mutexes
  /// </summary>
  [Serializable]
  public class LockingException : SkyException
  {
    public LockingException() : base() { }
    public LockingException(string message) : base(message) { }
    public LockingException(string message, Exception inner) : base(message, inner) { }
    protected LockingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
