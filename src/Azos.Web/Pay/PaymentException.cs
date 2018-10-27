/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
