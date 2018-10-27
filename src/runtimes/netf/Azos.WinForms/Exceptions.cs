/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

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
