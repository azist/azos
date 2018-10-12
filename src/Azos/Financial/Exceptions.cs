
using System;
using System.Runtime.Serialization;

namespace Azos.Financial
{
  /// <summary>
  /// Base exception thrown by the financial-related framework
  /// </summary>
  [Serializable]
  public class FinancialException : AzosException
  {
    public FinancialException() { }
    public FinancialException(string message) : base(message) { }
    public FinancialException(string message, Exception inner) : base(message, inner) { }
    protected FinancialException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
