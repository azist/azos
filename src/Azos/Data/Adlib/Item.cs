/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps.Injection;
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
  public sealed class Item : PersistedModel<ChangeResult>, IDistributedStableHashProvider
  {
    public Item() { }//serializer

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

    /// <summary>
    /// Optional data segment designator which you can use to sub-divide data within a collection
    /// </summary>
    [Field(required: false, Description = "Optional data segment designator which you can use to sub-divide data whiting a collection")]
    public int Segment {  get ; set; }

    /// <summary>
    /// Item GDID Primary Key which is unique per space/collection
    /// </summary>
    [Field(required: true, Description = "Item GDID Primary Key which is unique per space/collection")]
    public GDID Gdid { get; set; }

    /// <summary>
    /// Item EntityId using Gdid schema
    /// </summary>
    [Field(required: true, Description = "Item EntityId using Gdid schema")]
    public EntityId Id => Constraints.EncodeItemId(Space, Collection, Gdid);

    /// <summary>
    /// Optional Sharding topic which defines what shard within a space the item gets stored at.
    /// An unset/null property is equivalent to item's GDID for sharding
    /// </summary>
    [Field(maxLength: Constraints.MAX_SHARD_TOPIC_LEN, Description = "Sharding topic")]
    public string ShardTopic { get; set; }

    /// <summary>
    /// Returns an effective ShardKey based on either ShardTopic or string representation of a Gdid key
    /// </summary>
    public ShardKey EffectiveShardKey => ShardTopic.IsNotNullOrWhiteSpace() ? new ShardKey(ShardTopic) : new ShardKey(Constraints.GdidToShardKey(Gdid));


    /// <summary>
    /// Unix timestamp with ms resolution - when event was triggered at Origin
    /// </summary>
    [Field(required: true, Description = "Unix timestamp with ms resolution - when event was triggered at Origin")]
    public ulong CreateUtc { get; set; }

    /// <summary>
    /// The id of cluster origin region/zone where the item was first triggered, among other things
    /// this value is used to prevent circular traffic - in multi-master situations so the
    /// same event does not get replicated multiple times across regions (data centers)
    /// </summary>
    [Field(required: true, Description = "Id of cluster origin zone/region")]
    public Atom Origin { get; set; }

    /// <summary>Optional header content </summary>
    [Field(maxLength: Constraints.MAX_HEADERS_LENGTH, Description = "Optional header content")]
    public ConfigVector Headers { get; set; }

    /// <summary>Content type e.g. json</summary>
    [Field(Description = "Content type")]
    public Atom ContentType { get; set; }

    [Field(required: true, maxLength: Constraints.MAX_TAG_COUNT, Description = "Indexable tags")]
    public List<Tag> Tags { get; set; }

    /// <summary> Raw event content </summary>
    [Field(required: true, maxLength: Constraints.MAX_CONTENT_LENGTH, Description = "Raw event content")]
    public byte[] Content { get; set; }


    public override string ToString() => $"Item({Gdid})";
    public ulong GetDistributedStableHash() => EffectiveShardKey.GetDistributedStableHash();

    [Inject] IAdlibLogic m_Logic;
    [Inject(Optional = true)] IGdidProviderModule m_GdidModule;

    /// <summary>
    /// Excuses GDID from required validation until later, as it is generated on server on insert only
    /// </summary>
    public override ValidState ValidateField(ValidState state, Schema.FieldDef fdef, string scope = null)
      => (fdef.Name == nameof(Gdid) && FormMode == FormMode.Insert) ? state : base.ValidateField(state, fdef, scope);

    protected override Task DoBeforeSaveAsync()
    {
      if (FormMode == FormMode.Insert && Gdid.IsZero && m_GdidModule != null)
      {
        Gdid = Constraints.GenerateItemGdid(m_GdidModule.Provider, Space, Collection);
      }

      return Task.CompletedTask;
    }

    protected override async Task<SaveResult<ChangeResult>> DoSaveAsync()
     => new SaveResult<ChangeResult>(await m_Logic.SaveAsync(this).ConfigureAwait(false));
  }

}

