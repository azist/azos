/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps.Injection;
using Azos.Data.AST;
using Azos.Data.Business;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.Data.Adlib
{
  [Bix("5eadc54c-4637-40f3-9bb8-6266663bc525")]
  [Schema(Description = "Provides model for filtering of data items")]
  public sealed class ItemFilter : FilterModel<IEnumerable<Item>>
  {
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
    /// Optional Sharding topic which defines what shards the search is directed at
    /// </summary>
    [Field(maxLength: Constraints.MAX_SHARD_TOPIC_LEN, Description = "Optional Sharding topic which defines what shards the search is directed at")]
    public string ShardTopic { get; set; }

    /// <summary>
    /// Optional data segment designator which you can use to sub-divide data whiting a collection
    /// </summary>
    [Field(required: false, Description = "Optional data segment designator which you can use to sub-divide data whiting a collection")]
    public int? Segment { get; set; }

    /// <summary>
    /// Gets specific item by its GDID(within a space/collection)
    /// </summary>
    [Field(description: "Item GDID")]
    public GDID Gdid { get; set; }

    /// <summary>
    /// Optional create UTC date range filter
    /// </summary>
    [Field(required: false, Description = "Optional create UTC date range filter")]
    public DateRange? CreateDateRangeUtc { get; set; }


    [Field(description: "Fetches Item tags")]
    public bool FetchTags { get; set; }

    [Field(description: "Fetches Item content")]
    public bool FetchContent { get; set; }


    [Field(description: "Tag filter expression tree")]
    public Expression TagFilter { get; set; }

    [Field(description: "True to ignore partial reads when some shards may have failed, otherwise crashes " +
                        "the whole filter processing on any shard error (the default behavior)")]
    public bool IgnoreShardErrors { get; set; }

    [InjectModule] IAdlibLogic m_Logic;

    protected async override Task<SaveResult<IEnumerable<Item>>> DoSaveAsync()
     => new SaveResult<IEnumerable<Item>>(await m_Logic.GetListAsync(this).ConfigureAwait(false));
  }
}
