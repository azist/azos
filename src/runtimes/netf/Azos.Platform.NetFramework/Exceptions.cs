using System;
using System.Runtime.Serialization;

namespace Azos.Platform.Abstraction.NetFramework
{
  /// <summary>
  /// Base exception thrown for the PAL-related issues
  /// </summary>
  [Serializable]
  public class NetFrameworkPALException : PALException
  {
    public NetFrameworkPALException() {}
    public NetFrameworkPALException(string message) : base(message) {}
    public NetFrameworkPALException(string message, Exception inner) : base(message, inner) {}
    protected NetFrameworkPALException(SerializationInfo info, StreamingContext context) : base(info, context) {}
  }

}