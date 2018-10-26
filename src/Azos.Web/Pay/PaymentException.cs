
using System;
using System.Runtime.Serialization;

namespace Azos.Web.Pay
{
  /// <summary>
  /// General ancestor for payment-related exceptions
  /// </summary>
  [Serializable]
  public class PaymentException: WebException
  {
    public PaymentException() { }
    public PaymentException(string message) : base(message) { }
    public PaymentException(string message, Exception inner) : base(message, inner) { }
    protected PaymentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
