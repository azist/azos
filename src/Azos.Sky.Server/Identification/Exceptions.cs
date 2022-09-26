/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Azos.Sky.Identification
{
  /// <summary>
  /// Thrown to indicate GDID generation related problems
  /// </summary>
  [Serializable]
  public class GdidException : SkyException
  {
    public GdidException() : base() { }
    public GdidException(string message) : base(message) { }
    public GdidException(string message, Exception inner) : base(message, inner) { }
    protected GdidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
