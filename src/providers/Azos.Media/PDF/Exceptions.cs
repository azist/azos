/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Media.PDF
{
  /// <summary>
  /// Base exception thrown by the PDF framework
  /// </summary>
  [Serializable]
  public class PdfException : AzosException
  {
    public PdfException() { }
    public PdfException(string message) : base(message) { }
    public PdfException(string message, Exception inner) : base(message, inner) { }
    protected PdfException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}