/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Log;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Reads archives of Log.Message items. The implementation is thread-safe
  /// </summary>
  [ContentTypeSupport(LogMessageArchiveAppender.CONTENT_TYPE_LOG)]
  public sealed class LogMessageArchiveReader : ArchiveBixReader<Message>
  {
    public LogMessageArchiveReader(IVolume volume) : base(volume){ }

    public override Message MaterializeBix(BixReader reader)
    {
      Message result = null;

      if (reader.ReadBool())//if non-null message
      {
        result = new Message();

        var ngdid = reader.ReadNullableGDID();
        result.Gdid = ngdid.HasValue ? ngdid.Value : GDID.ZERO;

        result.Guid = reader.ReadGuid();

        var nrel = reader.ReadNullableGuid();
        result.RelatedTo = nrel.HasValue ? nrel.Value : Guid.Empty;

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
        { //this could be serialized using bix for better performance
          result.ExceptionData = JsonReader.ToDoc<WrappedExceptionData>(edata);
        }
      }

      return result;
    }
  }
}
