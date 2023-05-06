/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Azos.Conf;
using Azos.Data;
using Azos.Data.Adlib;
using Azos.Data.Business;

namespace Azos.Sky.Blob
{
  /// <summary>
  /// Contract for working with SKY BLOB file system.
  /// Blob is a uniquely identified "file" which can be accessed via a stream.
  /// Blob file systems stores named blobs/files with tags which can be used to query for file attributes such as path/name.
  /// </summary>
  public interface IBlobStore
  {
    /// <summary>
    /// Returns a list of all known blob data spaces on the server
    /// </summary>
    Task<IEnumerable<Atom>> GetSpaceNamesAsync();

    /// <summary>
    /// Returns volume ids known to the system for the specified space
    /// </summary>
    Task<IEnumerable<Atom>> GetVolumeNamesAsync(Atom space);

    /// <summary>
    /// Gets a list of blob info objects per specified filter, for example
    /// you can filter blobs by tags such as logical file names and paths
    /// </summary>
    Task<IEnumerable<BlobInfo>> FindBlobsAsync(BlobFilter filter);

    /// <summary>
    /// Create a new blob and opens a <see cref="BlobHandle"/> instance
    /// </summary>
    /// <param name="id">If of a blob in `volume@space::name` format <see cref="EntityId"/></param>
    /// <param name="permissions">Optional permissions section with 3 sub-sections: read/write/delete</param>
    /// <param name="headers">Optional headers to set</param>
    /// <param name="tags">Optional tags to set</param>
    /// <param name="endUtc">Optional absolute time of expiration</param>
    /// <param name="blockSize">Block size, can only set at create. 0 = default</param>
    /// <returns>Instance of <see cref="BlobHandle"/> which must be disposed</returns>
    Task<BlobHandle> CreateAsync(EntityId id,
                               IConfigSectionNode permissions = null,
                               IConfigSectionNode headers = null,
                               IEnumerable<Tag> tags = null,
                               DateTime? endUtc = null,
                               int blockSize = 0);

    /// <summary>
    /// Opens a <see cref="BlobHandle"/> for an existing blob. If does not exist returns null
    /// </summary>
    /// <param name="id">If of a blob in `volume@space::name` format <see cref="EntityId"/></param>
    /// <param name="readOnly">Open stream for read-only mode</param>
    /// <returns>Instance of <see cref="BlobHandle"/> which must be disposed</returns>
    Task<BlobHandle> OpenAsync(EntityId id, bool readOnly = false);

    /// <summary>
    /// Updates header/tag information of an existing blob.
    /// </summary>
    Task<ChangeResult> UpdateAsync(EntityId id, IConfigSectionNode headers, IEnumerable<Tag> tags, DateTime? endUtc);

    /// <summary>
    /// Deletes an existing blob
    /// </summary>
    Task<ChangeResult> DeleteAsync(EntityId id);

    //Rename file
  }

  public interface IBlobStoreLogic : IBlobStore, IBusinessLogic
  {
    /// <summary>
    /// Returns (null, null) if such blob is not found.
    /// The function returns data block UP to the specified count bytes in size.
    /// The maximum count is also capped at the value that the system deems reasonable for load leveling purposes which
    /// is typically a few hundred kilobytes
    /// </summary>
    Task<(VolatileBlobInfo info, byte[] data)> ReadBlockAsync(RGDID rgBlob, long offset, int count, CancellationToken cancellationToken);

    /// <summary>
    /// Returns null if such blob is not found
    /// </summary>
    Task<VolatileBlobInfo> WriteBlockAsync(RGDID rgBlob, long offset, ArraySegment<byte> buffer, CancellationToken cancellationToken);
  }
}
