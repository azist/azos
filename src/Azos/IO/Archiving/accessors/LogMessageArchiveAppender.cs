/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Azos.Data;
using Azos.Log;
using Azos.Serialization.Bix;
using Azos.Serialization.JSON;
using Azos.Time;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Appends into Azos log archives
  /// </summary>
  [ContentTypeSupport(CONTENT_TYPE_LOG)]
  public sealed class LogMessageArchiveAppender : ArchiveBixAppender<Message>
  {
    public const string CONTENT_TYPE_LOG = "bix/azlog";

    public LogMessageArchiveAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<Message, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit){ }

    protected override void DoSerializeBix(BixWriter wri, Message entry)
    {
      if (entry == null)
      {
        wri.Write(false); //NULL
        return;
      }

      wri.Write(true); // NON-NULL

      wri.Write(entry.Gdid.IsZero ? (GDID?)null : entry.Gdid);//nullable Gdid will consume 1 byte instead of 12 zeros
      wri.Write(entry.Guid);
      wri.Write(entry.RelatedTo == Guid.Empty ? (Guid?)null : entry.RelatedTo);//nullable Guid takes 1 byte instead of 16
      wri.Write(entry.Channel);
      wri.Write(entry.App);
      wri.Write((int)entry.Type);
      wri.Write(entry.Source);
      wri.Write(entry.UTCTimeStamp);

      wri.Write(entry.Host);
      wri.Write(entry.From);
      wri.Write(entry.Topic);
      wri.Write(entry.Text);
      wri.Write(entry.Parameters);
      wri.Write(entry.ArchiveDimensions);

      string edata = null;
      if (entry.ExceptionData != null)
      {
        edata = JsonWriter.Write(entry.ExceptionData, JsonWritingOptions.CompactRowsAsMap);
      }
      wri.Write(edata);
    }
  }
}
