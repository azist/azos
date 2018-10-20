
using System;
using System.Runtime.Serialization;

namespace Azos.WinForms
{
  /// <summary>
  /// Base exception thrown by Azos.WinForms assembly
  /// </summary>
  [Serializable]
  public class WFormsException : AzosException
  {
    public WFormsException() { }
    public WFormsException(string message) : base(message) { }
    public WFormsException(string message, Exception inner) : base(message, inner) { }
    protected WFormsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
