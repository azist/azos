/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;

namespace Azos.Web
{
  /// <summary>
  /// Base exception class thrown by Azos.Web assembly
  /// </summary>
  [Serializable]
  public class WebException : AzosException
  {
    public WebException() { }
    public WebException(string message) : base(message) { }
    public WebException(string message, Exception inner) : base(message, inner) { }
    protected WebException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
