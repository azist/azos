/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Data.Idgen;

namespace Azos.Data.Adlib
{
  /// <summary>
  /// Defines limits/constraints for data server
  /// </summary>
  public static class Constraints
  {
    public const string ID_NS_CONFIG_ADLIB_PREFIX = "az-adlib-";

    public const int MAX_HEADERS_LENGTH = 8 * 1024;
    public const int MAX_CONTENT_LENGTH = 10 * 1024 * 1024;
    public const int MAX_STRING_REPRESENTATION_CONTENT_LENGTH = MAX_CONTENT_LENGTH * 3;//hex encoding(2chars plus coma) or base64 encoding
    public const int MAX_TAG_COUNT = 128;
    public const int MAX_SHARD_TOPIC_LEN = 128;

    public const int MAX_TAG_SVAL_LENGTH = 250;

    public static readonly Atom SCH_GITEM = Atom.Encode("gi");

    public static EntityId EncodeItemId(Atom space, Atom collection, GDID gdid)
      => !space.IsZero && space.IsValid
           ? new EntityId(space, collection, Constraints.SCH_GITEM, gdid.ToString())
           : EntityId.EMPTY;


    public static (Atom space, Atom collection, GDID gdid) DecodeItemId(EntityId id)
    {
      (id.Schema == SCH_GITEM).IsTrue("Schema "+ SCH_GITEM);
      var gdid = id.Address.AsGDID();
      gdid.HasRequiredValue("Gdid");
      return (id.System, id.Type, gdid);
    }

    /// <summary>
    /// Returns a stable predictable string representation of GDID which can be used as a shard key
    /// </summary>
    public static string GdidToShardKey(GDID id) => id.ToString();

    /// <summary>
    /// Generates GDID for an item using appropriate gdid sequence names
    /// </summary>
    public static GDID GenerateItemGdid(IGdidProvider provider, Atom space, Atom collection)
    {
      var result = provider.NonNull(nameof(provider))
                           .GenerateOneGdid(ID_NS_CONFIG_ADLIB_PREFIX + space.HasRequiredValue(nameof(space)).Value,
                                                                        collection.HasRequiredValue(nameof(collection)).Value);
      return result;
    }
  }
}
