/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Sky.Server.Apps.Hosting.Skyod
{
  /// <summary>
  /// Provides (id, descr, createUtc) tuple describing software packages
  /// </summary>
  public struct PackageInfo : IEquatable<PackageInfo>, IJsonWritable, IJsonReadable, IRequiredCheck
  {
    public PackageInfo(string id, string description, DateTime createUtc)
    {
      Id = id; Description = description; CreateUtc = createUtc;
    }

    public readonly string Id;       //  g8-biz-78787897897.apar
    public readonly string Description;
    public readonly DateTime CreateUtc;

    public bool Assigned => Id.IsNotNullOrWhiteSpace();
    public bool CheckRequired(string targetName) => Assigned;
    public bool Equals(PackageInfo other) => Id.EqualsOrdSenseCase(other.Id);
    public override bool Equals(object obj) => obj is PackageInfo opi ? this.Equals(opi) : false;
    public override int GetHashCode() => Id == null ? 0 : Id.GetHashCode();

    public static bool operator ==(PackageInfo a, PackageInfo b) => a.Equals(b);
    public static bool operator !=(PackageInfo a, PackageInfo b) => !a.Equals(b);

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
      => JsonWriter.WriteMap(wri, nestingLevel + 1, options, new DictionaryEntry("id", Id),
                                                           new DictionaryEntry("desc", Description),
                                                           new DictionaryEntry("cutc", CreateUtc.ToMillisecondsSinceUnixEpochStart()));

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      if (data is JsonDataMap map)
      {
        var result = new PackageInfo(map["id"].AsString(), map["desc"].AsString(), map["cutc"].AsLong().FromMillisecondsSinceUnixEpochStart());
        return (true, result);
      }
      return (false, null);
    }
  }//PackageInfo
}
