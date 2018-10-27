/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Pile
{
  /// <summary>
  /// Thrown by pile memory manager
  /// </summary>
  [Serializable]
  public class PileException : AzosException
  {
    public PileException() { }
    public PileException(string message) : base(message) { }
    public PileException(string message, Exception inner) : base(message, inner) { }
    protected PileException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by pile memory manager when a supplied PilePointer is not pointing to a valid buffer
  /// </summary>
  [Serializable]
  public class PileAccessViolationException : PileException
  {
    public PileAccessViolationException() { }
    public PileAccessViolationException(string message) : base(message) { }
    public PileAccessViolationException(string message, Exception inner) : base(message, inner) { }
    protected PileAccessViolationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by pile memory manager when there is not anough room in the pile to perform the requested operation
  /// </summary>
  [Serializable]
  public class PileOutOfSpaceException : PileException
  {
    public PileOutOfSpaceException() { }
    public PileOutOfSpaceException(string message) : base(message) { }
    public PileOutOfSpaceException(string message, Exception inner) : base(message, inner) { }
    protected PileOutOfSpaceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown by pile cache
  /// </summary>
  [Serializable]
  public class PileCacheException : PileException
  {
    public PileCacheException() { }
    public PileCacheException(string message) : base(message) { }
    public PileCacheException(string message, Exception inner) : base(message, inner) { }
    protected PileCacheException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}