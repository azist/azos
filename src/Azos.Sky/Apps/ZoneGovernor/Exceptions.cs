using System;
using System.Runtime.Serialization;

namespace Azos.Sky.Apps.ZoneGovernor
{
  /// <summary>
  /// Thrown to indicate AZGOV related problems
  /// </summary>
  [Serializable]
  public class AZGOVException : SkyException
  {
    public AZGOVException() : base() { }
    public AZGOVException(string message) : base(message) { }
    public AZGOVException(string message, Exception inner) : base(message, inner) { }
    protected AZGOVException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
