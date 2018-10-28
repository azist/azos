using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Metabase
{
  /// <summary>
  /// Thrown to indicate metabase-related problems
  /// </summary>
  [Serializable]
  public class MetabaseException : SkyException
  {
    public MetabaseException() : base() { }
    public MetabaseException(string message) : base(message) { }
    public MetabaseException(string message, Exception inner) : base(message, inner) { }
    protected MetabaseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
