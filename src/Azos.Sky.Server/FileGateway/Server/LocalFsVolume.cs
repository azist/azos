/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Azos.Conf;
using Azos.Data;

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


    [Config] private string m_MountPath;


    /// <summary>
    /// Root mount path as of which the external pass is based
    /// </summary>
    public string MountPath => m_MountPath;


    private string getPhysicalPath(string volumePath)
    {
      volumePath.NonBlankMax(Constraints.MAX_PATH_TOTAL_LEN, nameof(volumePath));
      var fullPath = Path.Join(m_MountPath, volumePath);
      return fullPath;
    }

    private ItemInfo getItemInfo(string path)
    {
      var result = new ItemInfo();
      var volumePath = Path.GetRelativePath(MountPath, path);
      result.Path = new EntityId(ComponentDirector.Name, this.Name, Atom.ZERO, volumePath);

      if (Directory.Exists(path))
      {
        var di = new DirectoryInfo(path);
        result.Type = ItemType.Directory;
        result.CreateUtc = di.CreationTimeUtc;
        result.LastChangeUtc = di.LastWriteTimeUtc;
        result.Size = 0;
      }

      //File
      result.Type = ItemType.File;
      var fi = new FileInfo(path);
      result.CreateUtc =  fi.CreationTimeUtc;
      result.LastChangeUtc = fi.LastWriteTimeUtc;
      result.Size = fi.Length;
      return result;
    }


    public override Task<IEnumerable<ItemInfo>> GetItemListAsync(string volumePath, bool recurse)
    {
      var path = getPhysicalPath(volumePath);
      File.GetAttributes(path).HasFlag(FileAttributes.Directory).IsTrue("Directory path");
      var all = Directory.GetFileSystemEntries(path,  "*", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
      IEnumerable<ItemInfo> result = all.Select(one => getItemInfo(one)).ToArray();
      return Task.FromResult(result);
    }

    public override Task<ItemInfo> GetItemInfoAsync(string volumePath)
    {
      var path = getPhysicalPath(volumePath);
      var result = getItemInfo(path);
      return Task.FromResult(result);
    }

    public override Task<ItemInfo> CreateDirectoryAsync(string volumePath)
    {
      var path = getPhysicalPath(volumePath);
      Directory.CreateDirectory(path);
      var result = getItemInfo(path);
      return Task.FromResult(result);
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
