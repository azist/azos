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
