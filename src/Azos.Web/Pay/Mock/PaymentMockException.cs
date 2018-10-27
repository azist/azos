/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
