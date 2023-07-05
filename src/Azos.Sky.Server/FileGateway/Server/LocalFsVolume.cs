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
using Azos.Collections;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;
using Azos.Web;
using System.IO;

namespace Azos.Sky.FileGateway.Server
{
  /// <summary>
  /// Implements Volume based on a local file system access API
  /// </summary>
  public sealed class LocalFsVolume : Volume
  {
    internal LocalFsVolume(GatewaySystem director, IConfigSectionNode conf) : base(director, conf)
    {
      m_MountPath.NonBlank(nameof(MountPath));
      Directory.Exists(m_MountPath).IsTrue("Existing MountPath");
    }


    private string m_MountPath;


    /// <summary>
    /// Root mount path as of which the external pass is based
    /// </summary>
    public string MountPath => m_MountPath;


    public override Task<IEnumerable<ItemInfo>> GetItemListAsync(int recurseLevels)
    {
      throw new NotImplementedException();
    }

    public override Task<ItemInfo> GetItemInfoAsync(string volumePath)
    {
      throw new NotImplementedException();
    }

    public override Task<ItemInfo> CreateDirectoryAsync(string volumePath)
    {
      throw new NotImplementedException();
    }

    public override Task<ItemInfo> CreateFileAsync(string volumePath, CreateMode mode, long offset, byte[] content)
    {
      throw new NotImplementedException();
    }

    public override Task<bool> DeleteItemAsync(string volumePath)
    {
      throw new NotImplementedException();
    }

    public override Task<(byte[] data, bool eof)> DownloadFileChunkAsync(EntityId path, long offset, int size)
    {
      throw new NotImplementedException();
    }

    public override Task<bool> RenameItemAsync(EntityId path, string newPath)
    {
      throw new NotImplementedException();
    }

    public override Task<ItemInfo> UploadFileChunkAsync(EntityId path, long offset, byte[] content)
    {
      throw new NotImplementedException();
    }

  }
}
