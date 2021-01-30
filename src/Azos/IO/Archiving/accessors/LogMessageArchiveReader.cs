/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Log;
using Azos.Serialization.Bix;

namespace Azos.IO.Archiving
{
  public class LogMessageArchiveReader : ArchiveReader<Message>
  {
    public LogMessageArchiveReader(IVolume volume) : base(volume){ }

    [ThreadStatic] private static BufferSegmentReadingStream ts_Stream;

    public override Message Materialize(Entry entry)
    {
      if (entry.State != Entry.Status.Valid) return null;

      var stream = ts_Stream;
      if (stream == null)
      {
        stream = new BufferSegmentReadingStream();
        ts_Stream = stream;
      }

      stream.UnsafeBindBuffer(entry.Raw);
      var reader = new BixReader(stream);

      var has = reader.ReadBool();

      if (!has) return null;

      var result = new Message();

      //read.....

      stream.UnbindBuffer();

      return result;
    }
  }
}
