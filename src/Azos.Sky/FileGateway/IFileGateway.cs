/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Data;


namespace Azos.Sky.FileGateway
{
  /// <summary>
  /// Provides file upload/download services.
  /// The server exposes "systems" which contain named "volumes", then a file/dir is identified by
  /// a string path rooted at volumes. The system uses <see cref="EntityId"/> as full item path in a form: `volume@system::/path/dir/file`
  /// </summary>
  public interface IFileGateway
  {
    /// <summary>
    /// Retrieves a list of server systems which in turn contain named volumes
    /// </summary>
    Task<IEnumerable<Atom>> GetSystemsAsync();

    /// <summary>
    /// Retrieves a list of server volumes - named "drives" where the path is rooted at, per specific system
    /// </summary>
    Task<IEnumerable<Atom>> GetVolumesAsync(Atom system);

    /// <summary>
    /// Gets a listing of items at the specified path level
    /// </summary>
    Task<IEnumerable<ItemInfo>> GetItemListAsync(EntityId path, int recurseLevels = 0);

    /// <summary>
    /// Gets item info for a specific item or null if it does not exist
    /// </summary>
    Task<IEnumerable<ItemInfo>> GetItemInfoAsync(EntityId path);

    /// <summary>
    /// Create a directory. If it exists, then does nothing
    /// </summary>
    Task<ItemInfo> CreateDirectory(EntityId path);

    /// <summary>
    /// Creates a file and sets its content chunk at the specified offset
    /// </summary>
    Task<ItemInfo> CreateFile(EntityId path, CreateMode mode, long offset, byte[] content);

    /// <summary>
    /// Uploads file chunk at the specified offset. Path must exist and be a file (not a directory)
    /// </summary>
    Task<ItemInfo> UploadFileChunk(EntityId path, long offset, byte[] content);

    /// <summary>
    /// Downloads file chunk at the specified offset of specified size. Path must exist and be a file (not a directory).
    /// The system may return less than requested in `size`, in which case the caller assumes EOF condition (eof will be true).
    /// If size is less or equal to zero, then the system sends as much as it can, returning `eof` bool value appropriately
    /// </summary>
    Task<(byte[] data, bool eof)> DownloadFileChunk(EntityId path, long offset = 0, int size = 0);

    /// <summary>
    /// Deletes an item by path
    /// </summary>
    Task<bool> DeleteItem(EntityId path);

    /// <summary>
    ///Renames item in-place, this is an atomic operation which can be used for "remote transactions"
    ///when large file needs to be uploaded in chunks, you can then rename the file to a name which the
    ///other party cal "see at once" after the upload has been finalized
    /// </summary>
    Task<bool> RenameItem(EntityId path, EntityId newPath);
  }

  /// <summary>
  /// Outlines a contract for implementing logic of IFileGateway
  /// </summary>
  public interface IFileGatewayLogic : IFileGateway, IModule
  {
  }
}
