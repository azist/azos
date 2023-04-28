/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;
using Azos.Data.Adlib;
using Azos.Data.Business;
using Azos.Serialization.Bix;

namespace Azos.Sky.Blob
{
  /// <summary>
  /// Returned by server to open local BlobHandle
  /// </summary>
  [Bix("5756e918-f6a3-404d-a25e-bb44cc8a3075")]
  [Schema(Description = "Returned by server to open local BlobHandle")]
  public sealed class BlobHandleDescriptor : FragmentModel
  {
    /// <summary> Blob unique gdid per volume </summary>
    [Field(required: true, Description = "Blob unique rgdid per volume")]
    public RGDID RGdid { get; set; }

    /// <summary> Blob id</summary>
    [Field(required: true, Description = "Blob id")]
    public EntityId Id { get; set; }

    /// <summary> Block size</summary>
    [Field(required: true, Description = "Block size")]
    public int BlockSize { get; set; }

    [Field(required: false, Description = "Headers")]
    public ConfigVector Headers { get; set; }

    [Field(required: false, maxLength: Constraints.TAG_COUNT_MAX, Description = "Indexable tags")]
    public Tag[] Tags { get; set; }

    [Field(required: true, Description = "Created by")]
    public EntityId CreatedBy { get; set; }

    [Field(required: true, Description = "Created UTC")]
    public DateTime CreatedUtc { get; set; }

    [Field(required: true, Description = "End UTC")]
    public DateTime? EndUtc { get; set; }
  }
}
