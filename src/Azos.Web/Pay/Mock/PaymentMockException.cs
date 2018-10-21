
using System;

using System.Runtime.Serialization;

namespace Azos.Web.Pay.Mock
{
  /// <summary>
  /// Represents stripe specific payment exception
  /// </summary>
  [Serializable]
  public class PaymentMockException : PaymentException
  {
    public PaymentMockException(string message) : base(message) { }
    public PaymentMockException(string message, Exception inner) : base(message, inner) { }
    protected PaymentMockException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
