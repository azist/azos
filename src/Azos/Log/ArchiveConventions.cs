/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Log
{
  /// <summary>
  /// Provides helper methods for encoding and decoding ArchiveDimensions string in a terse JSON format -
  /// a form of a convention of ArchiveDimensions use with IArchiveLoggable
  /// </summary>
  public static class ArchiveConventions
  {
    private static readonly JsonWritingOptions AD_JSON_ENCODE_FORMAT = new (JsonWritingOptions.CompactRowsAsMap)
    {
      MapSkipNulls = true,
      MapSortKeys = true,
      ISODates = true,
      MaxNestingLevel = 4,//20230508 DKh
      EnableTypeHints = true,//#864 20230508 DKh
    };

    private static readonly JsonReadingOptions AD_JSON_DECODE_FORMAT = new (JsonReadingOptions.DefaultLimits)
    {
      EnableTypeHints = true,//#864 20230508 DKh
      MaxDepth = 4
    };

    private static readonly JsonWritingOptions SD_JSON_ENCODE_FORMAT = new(JsonWritingOptions.CompactRowsAsMap)
    {
      MapSkipNulls = false,//Keep nulls
      MapSortKeys = false,//Structured data does not sort on keys
      ISODates = true,
      MaxNestingLevel = 8,
      EnableTypeHints = true,//#864 20230508 DKh
    };

    private static readonly JsonReadingOptions SD_JSON_DECODE_FORMAT = new(JsonReadingOptions.DefaultLimits)
    {
      EnableTypeHints = true,//#864 20230508 DKh
      MaxDepth = 8
    };

    /// <summary> Maximum size of analytical fact archive dimensions </summary>
    public const int ANALYTICS_MAX_DIMENSIONS_CHAR_LEN = 512;

    /// <summary> Maximum size of analytical fact structured data/metrics </summary>
    public const int ANALYTICS_MAX_METRICS_CHAR_LEN = 2 * 1024;


    /// <summary>
    /// Used as a convention identifier.
    /// By this convention, all archive dimensions data must start with a hashbang: #!ad\n,
    /// otherwise the decode function will not parse it and return null
    /// </summary>
    public const string AD_HASHBANG = "#!ad\n";

    /// <summary>
    /// Used as a convention identifier.
    /// By this convention, all archived structured data must start with a hashbang: #!sd\n,
    /// otherwise the decode function will not parse it and return null
    /// </summary>
    public const string SD_HASHBANG = "#!sd\n";

    /// <summary>
    /// Returns a JsonDataMap parsed out of IArchiveLoggable.ArchiveDimensions content when it is set,
    /// or null. The function returns null for content which does not start with AD_HASHBANG
    /// which represents this convention. Throws on invalid JSON
    /// </summary>
    public static JsonDataMap DecodeArchiveDimensionsMap(this IArchiveLoggable entity)
    {
      if (entity==null) return null;
      var ad = entity.ArchiveDimensions;
      return DecodeArchiveDimensionsMap(ad);
    }

    /// <summary>
    /// Returns a JsonDataMap parsed out of ArchiveDimensions content when it is set,
    /// or null. The function returns null for content which does not start with AD_HASHBANG
    /// which represents this convention. Throws on invalid JSON
    /// </summary>
    public static JsonDataMap DecodeArchiveDimensionsMap(string archiveDimensions)
    {
      if (archiveDimensions.IsNullOrWhiteSpace()) return null;
      if (!archiveDimensions.StartsWith(AD_HASHBANG, StringComparison.Ordinal)) return null;
      var result = archiveDimensions.JsonToDataObject(AD_JSON_DECODE_FORMAT) as JsonDataMap;
      return result;
    }

    /// <summary>
    /// Returns a terse json object encoded into a string, or null if the supplied argument is null.
    /// The encoded string is prefixed with AD_HASHBANG to mark the content as the one
    /// encoded with this convention
    /// </summary>
    public static string EncodeArchiveDimensions(object archiveDimensions)
    {
      if (archiveDimensions == null) return null;
      return AD_HASHBANG + archiveDimensions.ToJson(AD_JSON_ENCODE_FORMAT);
    }


    /// <summary>
    /// Returns a JsonDataMap parsed out of structured data content when it is set,
    /// or null. The function returns null for content which does not start with SD_HASHBANG
    /// which represents this convention. Throws on invalid JSON
    /// </summary>
    public static JsonDataMap DecodeStructuredDataMap(string data)
    {
      if (data.IsNullOrWhiteSpace()) return null;
      if (!data.StartsWith(SD_HASHBANG, StringComparison.Ordinal)) return null;
      var result = data.JsonToDataObject(SD_JSON_DECODE_FORMAT) as JsonDataMap;
      return result;
    }

    /// <summary>
    /// Returns a terse json object encoded into a string, or null if the supplied argument is null.
    /// The encoded string is prefixed with SD_HASHBANG to mark the content as the one
    /// encoded with this convention
    /// </summary>
    public static string EncodeStructuredData(object data)
    {
      if (data == null) return null;
      return SD_HASHBANG + data.ToJson(SD_JSON_ENCODE_FORMAT);
    }

    /// <summary>
    /// Converts analytics fact data into <see cref="Message"/> suitable for processing in log archives/pipes, such as chronicle.
    /// Returns a message, you must to call `msg.InitDefaultFields(m_App);` if you need default values
    /// </summary>
    public static Message AnalyticsFactDataToLogMessage(Atom factType,
                                                        object dims,
                                                        object metrics,
                                                        int source = 0,
                                                        Guid rel = default(Guid),
                                                        string host = null,
                                                        MessageType messageType = MessageType.Info,
                                                        DateTime utcTimeStamp = default,
                                                        string topic = null,
                                                        Atom channel = default)
    {
      factType.HasRequiredValue(nameof(factType));

      var msg = new Message
      {
        Channel = channel.Default(CoreConsts.LOG_CHANNEL_ANALYTICS),
        Host = host,
        Topic = topic.Default(CoreConsts.LOG_TOPIC),
        Type = messageType,
        Source = source,
        From = factType.Value,
        RelatedTo = rel,
        UTCTimeStamp = utcTimeStamp
      };

      if (dims != null)
      {
        msg.ArchiveDimensions = ArchiveConventions.EncodeArchiveDimensions(dims)
                                                  .NonBlankMax(ANALYTICS_MAX_DIMENSIONS_CHAR_LEN, "dims.len < 512");
      }

      if (metrics != null)
      {
        msg.Parameters =  ArchiveConventions.EncodeStructuredData(metrics)
                                            .NonBlankMax(ANALYTICS_MAX_METRICS_CHAR_LEN, "metrics.len < 2k");
      }

      return msg;
    }

    /// <summary>
    /// Extracts Fact representation from log message
    /// </summary>
    public static Fact LogMessageToFact(Message msg, string targetName = null, Func<Message, string, Fact> factory = null)
    {
      if (msg == null) return null;
      var fact = factory != null ? factory(msg, targetName) : new Fact();


      if (Atom.TryEncodeValueOrId(msg.From, out var aFrom))
      {
        fact.FactType = aFrom;
      }
      fact.Id = msg.Guid;
      fact.Gdid = msg.Gdid;
      fact.RelatedId = msg.RelatedTo;
      fact.Channel = msg.Channel;
      fact.Topic = msg.Topic;
      fact.Host = msg.Host;
      fact.App = msg.App;
      fact.RecordType = msg.Type;
      fact.Source = msg.Source;
      fact.UtcTimestamp = msg.UTCTimeStamp;

      if (msg.ArchiveDimensions.IsNotNullOrWhiteSpace())
      {
        fact.Dimensions = ArchiveConventions.DecodeArchiveDimensionsMap(msg.ArchiveDimensions);
      }

      if (msg.Parameters.IsNotNullOrWhiteSpace())
      {
        fact.Metrics = ArchiveConventions.DecodeStructuredDataMap(msg.Parameters);
      }

      ((IAmorphousData)fact).AfterLoad(targetName);

      return fact;
    }

  }
}
