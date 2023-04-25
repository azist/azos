/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
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
    /// Returns a sequence of volume ids known to the system
    /// </summary>
    Task<IEnumerable<Atom>> GetVolumeNamesAsync();

    Task<IEnumerable<BlobInfo>> FindBlobsAsync(BlobFilter filter);

    /// <summary>
    /// Opens a <see cref="BlobHandle"/> for an existing blob, or creates anew blob.
    /// </summary>
    /// <param name="id">If of a blob in `volume@system::name` format <see cref="EntityId"/></param>
    /// <param name="headers">Optional headers to set if blob does not exist</param>
    /// <param name="tags">Optional tags to set if blob does not exist</param>
    /// <param name="bufferSize">Local buffer size. 0 = default</param>
    /// <param name="readOnly">Open stream for read-only mode</param>
    /// <returns>Instance of <see cref="BlobHandle"/> which must be disposed</returns>
    Task<BlobHandle> OpenAsync(EntityId id,
                               IConfigSectionNode headers = null,
                               IEnumerable<Tag> tags = null,
                               int bufferSize = 0,
                               bool readOnly = false);

    /// <summary>
    /// Updates header/tag information of an existing blob
    /// </summary>
    Task<ChangeResult> UpdateAsync(EntityId id, IConfigSectionNode headers, IEnumerable<Tag> tags);

    /// <summary>
    /// Deletes an existing blob
    /// </summary>
    Task<ChangeResult> DeleteAsync(EntityId id);
  }

  public interface IBlobStoreLogic : IBlobStore, IBusinessLogic
  {
  }
}
