/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Data;
using Azos.Log;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Appends into Azos fact binary archives
  /// </summary>
  [ContentTypeSupport(CONTENT_TYPE_FACTS)]
  public sealed class FactArchiveAppender : ArchiveBixAppender<Fact>
  {
    public const string CONTENT_TYPE_FACTS = "bix/azfacts";

    public FactArchiveAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<Fact, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit){ }

    protected override void DoSerializeBix(BixWriter wri, Fact entry)
    {
      if (entry == null)
      {
        wri.Write(false); //NULL
        return;
      }

      wri.Write(true); // NON-NULL
      wri.Write(entry.FactType);
      wri.Write(entry.Id);
      wri.Write(entry.RelatedId == Guid.Empty ? (Guid?)null : entry.RelatedId);//nullable Guid takes 1 byte instead of 16
      wri.Write(entry.Gdid.IsZero ? (GDID?)null : entry.Gdid);//nullable Gdid will consume 1 byte instead of 12 zeros
      wri.Write(entry.Channel);
      wri.Write(entry.Topic);
      wri.Write(entry.Host);
      wri.Write(entry.App);
      wri.Write((int)entry.RecordType);
      wri.Write(entry.Source);
      wri.Write(entry.UtcTimestamp);
      Bixon.WriteObject(wri, entry.HasAmorphousData ? entry.AmorphousData : null);
      Bixon.WriteObject(wri, entry.Dimensions);
      Bixon.WriteObject(wri, entry.Metrics);
    }
  }
}
