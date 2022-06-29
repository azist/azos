/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Data.Adlib
{
  /// <summary>
  /// Base exception thrown by Adlib
  /// </summary>
  [Serializable]
  public class AdlibException : DataException
  {
    public AdlibException() { }
    public AdlibException(string message) : base(message) { }
    public AdlibException(string message, Exception inner) : base(message, inner) { }
    protected AdlibException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

}
