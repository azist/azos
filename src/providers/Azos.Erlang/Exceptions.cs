/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Erlang
{
  /// <summary>
  /// Base exception for Erlang-related technology
  /// </summary>
  public class ErlException : AzosException, IQueable
  {
    public ErlException(string message) : base(message)
    { }

    public ErlException(string message, Exception inner) : base(message, inner)
    { }

    public ErlException(string message, params object[] args)
        : base(string.Format(message, args))
    { }

    public ErlException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
  }

  public class ErlIncompatibleTypesException : ErlException
  {
    public ErlIncompatibleTypesException(IErlObject lhs, Type rhs)
        : base(StringConsts.ERL_CANNOT_CONVERT_TYPES_ERROR, lhs.GetType().Name, rhs.Name)
    { }

    public ErlIncompatibleTypesException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    { }
  }

  /// <summary>
  /// Exception thrown when decoding an Erlang term if there's not enough
  /// data in the buffer to construct the term
  /// </summary>
  public class NotEnoughDataException : ErlException
  {
    public NotEnoughDataException() : base(StringConsts.ERL_CANNOT_READ_FROM_STREAM_ERROR) { }
    public NotEnoughDataException(string reason) : base(reason) { }
  }

  /// <summary>
  /// Exception thrown when the connection with a given node gets broken
  /// </summary>
  public class ErlConnectionException : ErlException
  {
    public ErlConnectionException(ErlAtom nodeName, string reason)
        : base(string.Empty)
    {
      Node = nodeName;
      Reason = new ErlString(reason);
    }

    public ErlConnectionException(ErlAtom nodeName, IErlObject reason)
        : base(string.Empty)
    {
      Node = nodeName;
      Reason = reason;
    }

    /// <summary>
    /// Name of the node that experienced connectivity loss
    /// </summary>
    public readonly ErlAtom Node;

    /// <summary>
    /// Get the reason associated with this exit signal
    /// </summary>
    public readonly IErlObject Reason;

    public override string Message { get { return Reason.ToString(); } }
  }

  public class ErlBadDataException : ErlConnectionException
  {
    public ErlBadDataException(ErlAtom nodeName, string reason)
        : base(nodeName, reason)
    { }

    public ErlBadDataException(ErlAtom nodeName, IErlObject reason)
        : base(nodeName, reason)
    { }
  }

  /// <summary>
  /// Special message sent when a linked pid dies
  /// </summary>
  internal class ErlExit : ErlConnectionException
  {
    public ErlExit(ErlPid pid, string reason) : this(pid, (IErlObject)new ErlString(reason)) { }

    public ErlExit(ErlPid pid, IErlObject reason)
        : base(pid.Node, reason)
    {
      Pid = pid;
    }

    /// <summary>
    /// The pid that sent this exit
    /// </summary>
    public readonly ErlPid Pid;
  }

  /// <summary>
  /// Message sent when the monitored Pid dies
  /// </summary>
  internal class ErlDown : ErlExit
  {
    public ErlDown(ErlRef eref, ErlPid pid, IErlObject reason)
        : base(pid, reason)
    {
      Ref = eref;
    }

    public readonly ErlRef Ref;
  }

  public class ErlAuthException : ErlConnectionException
  {
    public ErlAuthException(ErlAtom nodeName, string reason) : base(nodeName, reason) { }
    public ErlAuthException(ErlAtom nodeName, IErlObject reason) : base(nodeName, reason) { }
  }
}
