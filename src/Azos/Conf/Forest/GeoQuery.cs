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
using Azos.Data;
using Azos.Data.Business;
using Azos.Geometry;
using Azos.Serialization.Bix;
using Azos.Standards;

namespace Azos.Conf.Forest
{
  /// <summary>
  /// Embodies GIS query data - either a full/partial address or LatLng location.
  /// The query object gets submitted into the conf tree for processing yielding a list of nodes which satisfy query conditions
  /// </summary>
  [Bix("ad9af904-cb4b-4816-ad37-05c550c97770")]
  [Schema(Description = "Geo query request by address/lat lng")]
  public sealed class GeoQuery : FilterModel<IEnumerable<TreeNodeInfo>>
  {
    /// <summary>
    /// Returns a forest id (EntityId.System) of the forest which contains this tree which contains this node
    /// </summary>
    [Field(required: true, Description = "Returns a forest id (EntityId.System) of the forest which contains this tree which contains this node")]
    public Atom Forest { get; set; }

    /// <summary>
    /// Returns tree id which contains this node
    /// </summary>
    [Field(required: true, Description = "Returns tree id which contains this node")]
    public Atom Tree { get; set; }

    [Field(required: false, Description = "Timestamp as of which this tree node becomes effective")]
    public DateTime? AsOfUtc { get; set; }

    [Field(required: false, Description = "Search radius around the specified location, if not specified then zero assumed")]
    public Distance? SearchRadius { get; set; }

    [Field(Description = "Country ISO code")]
    public Atom   Country { get; set; }

    [Field(Description = "Address line 1")]
    public string Line1 { get; set; }

    [Field(Description = "Address line 2 (optional)")]
    public string Line2 { get; set; }

    [Field(Description = "City/Municipality name")]
    public string City  { get; set; }

    [Field(Description = "County/Parish name")]
    public string County  { get; set; }

    [Field(Description = "State/Province/Territory name")]
    public string StateProvince { get; set; }

    [Field(Description = "Zip/Postal code")]
    public string PostalCode    { get; set; }

    [Field(Description = "Optional precise Location if known")]
    public LatLng? Location { get; set; }

    /// <summary>
    /// If greater than 0 then would allow reading a cached result for up-to the specified number of seconds.
    /// If =0 uses cache's default span.
    /// Less than 0 does not try to read from cache
    /// </summary>
    [Field(Description = "If greater than 0 then would allow reading a cached result for up-to the specified number of seconds." +
                         "If =0 uses cache's default span. Less than 0 does not try to read from cache")]
    public int CacheMaxAgeSec { get; set; }

    [InjectModule] IForestLogic m_Logic;

    protected async override Task<SaveResult<IEnumerable<TreeNodeInfo>>> DoSaveAsync()
     => new SaveResult<IEnumerable<TreeNodeInfo>>(await m_Logic.ExecGeoQueryAsync(this).ConfigureAwait(false));
  }
}
