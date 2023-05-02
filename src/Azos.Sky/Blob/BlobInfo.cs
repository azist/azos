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
  /// Provides blob information
  /// </summary>
  [Bix("a7f09b1a-e4cb-4d4d-a391-b30728552f4f")]
  [Schema(Description = "Provides blob information")]
  public sealed class BlobInfo : FragmentModel
  {
    /// <summary>
    /// Blob unique gdid per volume
    /// </summary>
    [Field(required: true, Description = "Blob unique gdid per volume")]
    public RGDID RGdid { get; set; }

    [Field(required: true, maxLength: Constraints.TAG_COUNT_MAX, Description = "Indexable tags")]
    public List<Tag> Tags { get; set; }


    [Field(required: true, Description = "When the blob was created")]
    public DateTime CreatedUtc { get; set; }

    [Field(required: true, Description = "Who created blob")]
    public EntityId CreatedBy { get; set; }

    [Field(required: true, Description = "Blob volatile information")]
    public VolatileBlobInfo Volatile { get; set; }
  }

  /// <summary>
  /// Provides blob volatile information
  /// </summary>
  [Bix("535e48ae-6355-4aba-ad7d-258658b1b8c0")]
  [Schema(Description = "Provides blob volatile information, such as modify stamps and size")]
  public sealed class VolatileBlobInfo : FragmentModel
  {
    [Field(required: true, Description = "Total blob length")]
    public long TotalLength { get; set; }


    [Field(required: true, Description = "When the blob was created")]
    public DateTime ModifiedUtc { get; set; }

    [Field(required: true, Description = "Who created blob")]
    public EntityId ModifiedBy { get; set; }

    internal void ReadFromBix(BixReader reader)
    {
      var ver = reader.ReadInt();//may use ver later to support upgrades down below
      TotalLength = reader.ReadLong();
      ModifiedUtc = reader.ReadDateTime();
      ModifiedBy = reader.ReadEntityId();
    }

    internal void WriteToBix(BixWriter writer)
    {
      const int VER = 1;
      writer.Write(VER);
      writer.Write(TotalLength);
      writer.Write(ModifiedUtc);
      writer.Write(ModifiedBy);
    }
  }
}
