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
  /// Reads archives of Log.Fact items. The implementation is thread-safe
  /// </summary>
  [ContentTypeSupport(FactArchiveAppender.CONTENT_TYPE_FACTS)]
  public sealed class FactArchiveReader : ArchiveBixReader<Fact>
  {
    public FactArchiveReader(IVolume volume, Func<Fact, Fact> factory = null) : base(volume)
    {
      Factory = factory;
    }

    public readonly Func<Fact, Fact> Factory;

    [ThreadStatic]
    private static Fact ts_FactCache;

    public override Fact MaterializeBix(BixReader reader)
    {
      if (!reader.ReadBool()) return null;

      Fact fact = null;

      if (Factory != null)
      {
        fact = ts_FactCache;
        if (fact == null)
        {
          fact = new Fact();
        }
      }
      else
      {
        fact = new Fact();
      }

      fact.FactType = reader.ReadAtom();
      fact.Id = reader.ReadGuid();

      var guid = reader.ReadNullableGuid();
      fact.RelatedId = guid.HasValue ? guid.Value : Guid.Empty;

      var gdid = reader.ReadNullableGDID();
      fact.Gdid = gdid.HasValue ? gdid.Value : GDID.ZERO;

      fact.Channel = reader.ReadAtom();
      fact.Topic = reader.ReadAtom();
      fact.Host = reader.ReadString();
      fact.App = reader.ReadAtom();
      fact.RecordType = (MessageType)reader.ReadInt();
      fact.Source = reader.ReadInt();
      fact.UtcTimestamp = reader.ReadDateTime();
      fact.SetAmorphousData(Bixon.ReadObject(reader) as JsonDataMap);
      fact.Dimensions = Bixon.ReadObject(reader) as JsonDataMap;
      fact.Metrics = Bixon.ReadObject(reader) as JsonDataMap;

      if (Factory != null)
      {
        var result = Factory(fact);
        ts_FactCache = object.ReferenceEquals(result, fact) ? null : fact;
        return result;
      }

      return fact;
    }
  }
}
