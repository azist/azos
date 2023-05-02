/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Serialization.JSON;

namespace Azos.Log
{
  /// <summary>
  /// Provides helper methods for encoding and decoding ArchiveDimensions string in a terse JSON format -
  /// a form of a convention of ArchiveDimensions use with IArchiveLoggable
  /// </summary>
  public static class ArchiveConventions
  {
    private static readonly JsonWritingOptions JSON_FORMAT = new JsonWritingOptions(JsonWritingOptions.CompactRowsAsMap)
    {
      MapSkipNulls = true,
      MapSortKeys = true,
      ISODates = true
    };

    /// <summary>
    /// Used as a convention identifier.
    /// By this convention, all archive dimensions data must start with a hashbang: #!ad\n,
    /// otherwise the decode function will not parse it and return null
    /// </summary>
    public const string AD_HASHBANG = "#!ad\n";

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
      var result = archiveDimensions.JsonToDataObject(JsonReadingOptions.Default) as JsonDataMap;
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
      return AD_HASHBANG + archiveDimensions.ToJson(JSON_FORMAT);
    }

  }
}
