/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azos.Data;
using Azos.Data.Business;
using Azos.Data.Idgen;
using Azos.Serialization.Bix;

namespace Azos.Data.Adlib
{
  /// <summary>
  /// Item entity stored in Amorphous Data Library server
  /// </summary>
  [Bix("a05d1e2d-e4b5-4fc7-a3bf-88356f9818dc")]
  [Schema(Description = "Item entity stored in Amorphous Data Library server")]
  public sealed class Item : PersistedEntity<IAdlibLogic, ChangeResult>, IDistributedStableHashProvider
  {
    internal Item() { }//serializer

    /// <summary>
    /// Returns a space id (EntityId.System) which contains item collection
    /// </summary>
    [Field(required: true, Description = "Returns a space id (EntityId.System) which contains item collection")]
    public Atom Space { get; set; }

    /// <summary>
    /// Collection within a space
    /// </summary>
    [Field(required: true, Description = "Collection within a space")]
    public Atom Collection { get; set; }

    public override EntityId Id => !Space.IsZero && Space.IsValid
                                     ? new EntityId(Space, Collection, Constraints.SCH_GITEM, this.Gdid.ToString())
                                     : EntityId.EMPTY;


    [Field(required: true, maxLength: Constraints.MAX_SHARD_TOPIC_LEN, Description = "Sharding topic")]
    public string ShardTopic { get; internal set; }

    /// <summary>
    /// Unix timestamp with ms resolution - when event was triggered at Origin
    /// </summary>
    [Field(required: true, Description = "Unix timestamp with ms resolution - when event was triggered at Origin")]
    public ulong CreateUtc { get; internal set; }

    /// <summary>
    /// The id of cluster origin region/zone where the event was first triggered, among other things
    /// this value is used to prevent circular traffic - in multi-master situations so the
    /// same event does not get replicated multiple times across regions (data centers)
    /// </summary>
    [Field(required: true, Description = "Id of cluster origin zone/region")]
    public Atom Origin { get; internal set; }

    /// <summary>Optional header content </summary>
    [Field(maxLength: Constraints.MAX_HEADERS_LENGTH, Description = "Optional header content")]
    public ConfigVector Headers { get; internal set; }

    /// <summary>Content type e.g. json</summary>
    [Field(Description = "Content type")]
    public Atom ContentType { get; internal set; }

    [Field(required: true, maxLength: Constraints.MAX_TAG_COUNT, Description = "Indexable tags")]
    public List<Tag> Tags { get; set; }

    /// <summary> Raw event content </summary>
    [Field(required: true, maxLength: Constraints.MAX_CONTENT_LENGTH, Description = "Raw event content")]
    public byte[] Content { get; internal set; }


    public override string ToString() => $"Item({Gdid})";
    public ulong GetDistributedStableHash() => ShardKey.ForString(ShardTopic);

    protected override async Task<ChangeResult> SaveBody(IAdlibLogic logic)
     => await logic.SaveAsync(this).ConfigureAwait(false);
  }

}

