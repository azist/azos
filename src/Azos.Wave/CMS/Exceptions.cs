
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

using Azos.Web;

namespace Azos.Wave.CMS
{
  /// <summary>
  /// Base exception thrown by the WAVE.CMS framework
  /// </summary>
  [Serializable]
  public class CMSException : WaveException
  {
    public CMSException() { }
    public CMSException(string message) : base(message) { }
    public CMSException(string message, Exception inner) : base(message, inner) { }
    protected CMSException(SerializationInfo info, StreamingContext context): base(info, context) { }
  }

}