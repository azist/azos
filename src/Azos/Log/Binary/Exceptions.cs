
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