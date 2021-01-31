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
using Azos.Serialization.JSON;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Reads archives of Log.Message items. The implementation is thread-safe
  /// </summary>
  public sealed class LogMessageArchiveReader : ArchiveReader<Message>
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


      Message result = null;

      if (reader.ReadBool())//if non-null message
      {
        result = new Message();
        result.Gdid = reader.ReadGDID();
        result.Guid = reader.ReadGuid();
        result.RelatedTo = reader.ReadGuid();
        result.Channel = reader.ReadAtom();
        result.App = reader.ReadAtom();
        result.Type = (MessageType)reader.ReadInt();
        result.Source = reader.ReadInt();
        result.UTCTimeStamp = reader.ReadDateTime();

        result.Host = reader.ReadString();
        result.From = reader.ReadString();
        result.Topic = reader.ReadString();
        result.Text = reader.ReadString();
        result.Parameters = reader.ReadString();
        result.ArchiveDimensions = reader.ReadString();

        var edata = reader.ReadString();
        if (edata != null)
        {
          result.ExceptionData = JsonReader.ToDoc<WrappedExceptionData>(edata);
        }
      }

      stream.UnbindBuffer();

      return result;
    }
  }
}
