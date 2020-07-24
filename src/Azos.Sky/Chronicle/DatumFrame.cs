/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Sky.Chronicle
{
  /// <summary>
  /// Provides a sealed DTO representation of CLR/platform-agnostic chronicle measurement sample.
  /// The data contract does not contain any platform-specific features, and as such can be used
  /// in cross-platform applications, e.g. instrumentation consumer/producers may all be implemented in
  /// different languages, exchanging common (such as JSON) representation of DatumFrame
  /// </summary>
  public sealed class DatumFrame : IJsonWritable, IJsonReadable
  {
    public DatumFrame(GDID gdid, Guid tp, string src, long cnt, DateTime sd, DateTime ed, double v, Atom ctp, byte[] content)
    {
      Gdid = gdid;
      Type = tp;
      Source = src;
      Count = cnt;
      StartUtc = sd;
      EndUtc = ed;
      RefValue = v;
      ContentType = ctp;
      Content = content;
    }

    /// <summary> Unique GDID assigned by chronicle. ZERO if not assigned yet </summary>
    public GDID Gdid { get; private set; }

    /// <summary> Unique Type ID of the Datum type </summary>
    public Guid Type { get; private set; }

    /// <summary> Source/Archive dimensions which are used to index the data </summary>
    public string Source { get; private set; } // #!ad {a:1, b:2, h:red}

    /// <summary> Count of measurements within measurement date range </summary>
    public long Count { get; private set; }

    /// <summary> Measurement date range start UTC </summary>
    public DateTime StartUtc { get; private set; }

    /// <summary> Measurement date range end UTC </summary>
    public DateTime EndUtc { get; private set; }

    /// <summary>
    /// Reference value is a scalar value representation of datum ValueAsObject.
    /// It is used for relative comparison. For complex values it returns a relative
    /// weighted reference value which can be used for querying chronicles on the reference ranges.
    /// For example: for blood pressure with two components this can return the 1-10
    /// "threat scale" which can calculate based on the higher of the systolic and diastolic components.
    /// </summary>
    public double? RefValue { get; private set; }

    /// <summary> Optional content encoding/type identifier stating the type of content serializer used, e.g. `bix` or `json`</summary>
    public Atom ContentType { get; private set; }

    /// <summary> Optional content of the datum encoded as ContentType </summary>
    public byte[] Content { get; private set; }

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
    {
      //todo Future optimization: this can be directly written to wri without extra allocations
      var map = new JsonDataMap
      {
        {"g", Gdid},
        {"t", Type},
        {"s", Source},
        {"c", Count},
        {"sd", StartUtc},
        {"ed", EndUtc},
        {"v", RefValue}
      };

      if (!ContentType.IsZero) map["ctp"] = ContentType;
      if (Content!=null) map["bin"] = Content.ToWebSafeBase64();

      JsonWriter.WriteMap(wri, map, nestingLevel+1, options);
    }

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      const System.Globalization.DateTimeStyles UTC = System.Globalization.DateTimeStyles.AssumeUniversal |
                                                      System.Globalization.DateTimeStyles.AdjustToUniversal;
      if (data is JsonDataMap map)
      {
        Gdid   = map["g"].AsGDID();
        Type   = map["t"].AsGUID(Guid.Empty);
        Source = map["s"].AsString();
        Count  = map["c"].AsLong();
        StartUtc = map["sd"].AsDateTime(UTC);
        EndUtc   = map["ed"].AsDateTime(UTC);
        RefValue = map["v"].AsNullableDouble();

        ContentType = map["ctp"].AsAtom();
        Content     = map["bin"].AsString().TryFromWebSafeBase64();
        return (true, this);
      }

      return (false, null);
    }
  }
}
