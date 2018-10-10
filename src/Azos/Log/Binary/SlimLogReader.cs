
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Serialization.Slim;

namespace Azos.Log.Binary
{
  /// <summary>
  /// Reads bin log in Slim format. The format does not support object versioning however it is
  /// very efficient in both space and speed
  /// </summary>
  public class SlimLogReader : LogReader
  {
    public SlimLogReader(Stream stream) : base(stream)
    {

    }


  }
}
