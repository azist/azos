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
    public GDID Gdid { get; set; }

    [Field(required: true, maxLength: Constraints.MAX_TAG_COUNT, Description = "Indexable tags")]
    public List<Tag> Tags { get; set; }


    [Field(required: true, Description = "When the file was created")]
    public DateTime CreateUtc { get; set; }

  }
}
