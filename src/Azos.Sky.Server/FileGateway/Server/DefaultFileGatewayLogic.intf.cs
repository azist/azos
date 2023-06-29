/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Client;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Sky.FileGateway.Server
{
  partial class DefaultFileGatewayLogic
  {
    public Task<IEnumerable<Atom>> GetSystemsAsync()
    {
      //todo Check permission
      return Task.FromResult(m_Systems.Names);
    }

    public Task<IEnumerable<Atom>> GetVolumesAsync(Atom system)
    {
      //todo check permissions
      var sys = this[system];
      return Task.FromResult(sys.Volumes.Names);
    }

    public Task<IEnumerable<ItemInfo>> GetItemListAsync(EntityId path, int recurseLevels = 0)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<ItemInfo>> GetItemInfoAsync(EntityId path)
    {
      throw new NotImplementedException();
    }


    public Task<ItemInfo> CreateDirectory(EntityId path)
    {
      throw new NotImplementedException();
    }

    public Task<ItemInfo> CreateFile(EntityId path, CreateMode mode, long offset, byte[] content)
    {
      throw new NotImplementedException();
    }

    public Task<bool> DeleteItem(EntityId path)
    {
      throw new NotImplementedException();
    }

    public Task<(byte[] data, bool eof)> DownloadFileChunk(EntityId path, long offset = 0, int size = 0)
    {
      throw new NotImplementedException();
    }

    public Task<bool> RenameItem(EntityId path, EntityId newPath)
    {
      throw new NotImplementedException();
    }

    public Task<ItemInfo> UploadFileChunk(EntityId path, long offset, byte[] content)
    {
      throw new NotImplementedException();
    }
  }
}
