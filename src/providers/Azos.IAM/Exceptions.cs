using System;
using System.Runtime.Serialization;
using System.Security;

namespace Azos.IAM
{
  /// <summary>
  /// Base exception thrown by the Azos.IAM framework
  /// </summary>
  [Serializable]
  public class IAMException : Azos.Security.SecurityException
  {
    public IAMException() { }
    public IAMException(string message) : base(message) { }
    public IAMException(string message, Exception inner) : base(message, inner) { }
    protected IAMException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }


}
