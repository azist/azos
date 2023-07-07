/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Data;

using Azos.Security.FileGateway;

namespace Azos.Sky.FileGateway.Server
{
  partial class DefaultFileGatewayLogic
  {
    public Task<IEnumerable<Atom>> GetSystemsAsync()
    {
      App.Authorize(new FileGatewayPermission());
      return Task.FromResult(m_Systems.Names);
    }

    public Task<IEnumerable<Atom>> GetVolumesAsync(Atom system)
    {
      App.Authorize(new FileGatewayPermission());
      var sys = this[system];
      return Task.FromResult(sys.Volumes.Names);
    }

    public async Task<IEnumerable<ItemInfo>> GetItemListAsync(EntityId path, bool recurse = false)
    {
      var vol = getVolume(path);
      App.Authorize(new FileGatewayPermission(FileGatewayAccessLevel.Read, path.System, path.Type));
      var result = await vol.GetItemListAsync(path.Address, recurse).ConfigureAwait(false);
      return result;
    }

    public async Task<ItemInfo> GetItemInfoAsync(EntityId path)
    {
      var vol = getVolume(path);
      App.Authorize(new FileGatewayPermission(FileGatewayAccessLevel.Read, path.System, path.Type));
      var result = await vol.GetItemInfoAsync(path.Address).ConfigureAwait(false);
      return result;
    }


    public async Task<ItemInfo> CreateDirectoryAsync(EntityId path)
    {
      var vol = getVolume(path);
      App.Authorize(new FileGatewayPermission(FileGatewayAccessLevel.Write, path.System, path.Type));
      var result = await vol.CreateDirectoryAsync(path.Address).ConfigureAwait(false);
      return result;
    }

    public async Task<ItemInfo> CreateFileAsync(EntityId path, CreateMode mode, long offset, byte[] content)
    {
      (offset >= 0).IsTrue("offset >= 0");
      content.NonNull(nameof(content));
      (content.Length < Constraints.MAX_FILE_CHUNK_SIZE).IsTrue($"contet.len < {Constraints.MAX_FILE_CHUNK_SIZE}");

      var vol = getVolume(path);
      App.Authorize(new FileGatewayPermission(FileGatewayAccessLevel.Write, path.System, path.Type));
      var result = await vol.CreateFileAsync(path.Address, mode, offset, content).ConfigureAwait(false);
      return result;
    }

    public async Task<bool> DeleteItemAsync(EntityId path)
    {
      var vol = getVolume(path);
      App.Authorize(new FileGatewayPermission(FileGatewayAccessLevel.Full, path.System, path.Type));
      var result = await vol.DeleteItemAsync(path.Address).ConfigureAwait(false);
      return result;
    }

    public async Task<(byte[] data, bool eof)> DownloadFileChunkAsync(EntityId path, long offset = 0, int size = 0)
    {
      (offset >= 0).IsTrue("offset >= 0");
      (size > 0 && size < Constraints.MAX_FILE_CHUNK_SIZE).IsTrue($"0 < size < {Constraints.MAX_FILE_CHUNK_SIZE}");

      App.Authorize(new FileGatewayPermission(FileGatewayAccessLevel.Read, path.System, path.Type));
      var vol = getVolume(path);
      var result = await vol.DownloadFileChunkAsync(path.Address, offset, size).ConfigureAwait(false);
      return result;
    }

    public async Task<bool> RenameItemAsync(EntityId path, string newPath)
    {
      newPath.NonBlankMax(Constraints.MAX_PATH_TOTAL_LEN, nameof(newPath));
      var vol = getVolume(path);
      App.Authorize(new FileGatewayPermission(FileGatewayAccessLevel.Write, path.System, path.Type));
      var result = await vol.RenameItemAsync(path.Address, newPath).ConfigureAwait(false);
      return result;
    }

    public async Task<ItemInfo> UploadFileChunkAsync(EntityId path, long offset, byte[] content)
    {
      (offset >= 0).IsTrue("offset >= 0");
      content.NonNull(nameof(content));
      (content.Length < Constraints.MAX_FILE_CHUNK_SIZE).IsTrue($"contet.len < {Constraints.MAX_FILE_CHUNK_SIZE}");

      var vol = getVolume(path);
      App.Authorize(new FileGatewayPermission(FileGatewayAccessLevel.Write, path.System, path.Type));
      var result = await vol.UploadFileChunkAsync(path.Address, offset, content).ConfigureAwait(false);
      return result;
    }
  }
}
