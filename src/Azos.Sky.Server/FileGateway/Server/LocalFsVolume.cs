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
      volumePath = volumePath.Trim();
      (volumePath.IndexOf(':') < 0).IsTrue("No ':'");
      (volumePath.IndexOf("..") < 0).IsTrue("No '..'");
      (!volumePath.Contains(@"\\") && !volumePath.Contains(@"//")).IsTrue("No UNC");

      var fullPath = Path.Join(m_MountPath, volumePath);
      return fullPath;
    }

    private ItemInfo getItemInfo(string fullLocalPath)
    {
      var result = new ItemInfo();
      try
      {
        var volumePath = Path.GetRelativePath(MountPath, fullLocalPath);
        result.Path = new EntityId(ComponentDirector.Name, this.Name, Atom.ZERO, volumePath);

        if (Directory.Exists(fullLocalPath))
        {
          var di = new DirectoryInfo(fullLocalPath);
          result.Type = ItemType.Directory;
          result.CreateUtc = di.CreationTimeUtc;
          result.LastChangeUtc = di.LastWriteTimeUtc;
          result.Size = 0;
        }

        //File
        result.Type = ItemType.File;
        var fi = new FileInfo(fullLocalPath);
        result.CreateUtc =  fi.CreationTimeUtc;
        result.LastChangeUtc = fi.LastWriteTimeUtc;
        result.Size = fi.Length;
        return result;
      }
      catch(Exception error)
      {
        var got = new FileGatewayException($"getItemInfo(`{fullLocalPath}`): {error.Message}", error);
        WriteLogFromHere(Azos.Log.MessageType.Error, got.ToMessageWithType(), got);
        throw got;
      }
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

    public override async Task<ItemInfo> CreateFileAsync(string volumePath, CreateMode mode, long offset, byte[] content)
    {
      var path = getPhysicalPath(volumePath);

      var fmode = mode == CreateMode.Create  ? FileMode.CreateNew :
                  mode == CreateMode.Replace ? FileMode.Create    :
                                               FileMode.OpenOrCreate;
      try
      {
        using(var fs = new FileStream(path, fmode, FileAccess.Write, FileShare.None))
        {
          fs.Position = offset;
          if (content != null)
          {
            await fs.WriteAsync(content, 0, content.Length).ConfigureAwait(false);
          }
        }
      }
      catch(Exception error)
      {
        var got = new FileGatewayException($"CreateFileAsync(`{volumePath}`, {mode}, {offset}, byte[{content?.Length ?? -1}]): {error.Message}", error);
        WriteLogFromHere(Azos.Log.MessageType.Error, got.ToMessageWithType(), got);
        throw got;
      }

      return getItemInfo(path);
    }

    public override Task<bool> DeleteItemAsync(string volumePath)
    {
      var path = getPhysicalPath(volumePath);
      try
      {
        if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
        {
          if (!Directory.Exists(path)) return Task.FromResult(false);

          Directory.Delete(path, true);
        }
        else
        {
          if (!File.Exists(path)) return Task.FromResult(false);
          File.Delete(path);
        }
      }
      catch (Exception error)
      {
        var got = new FileGatewayException($"DeleteItemAsync(`{volumePath}`): {error.Message}", error);
        WriteLogFromHere(Azos.Log.MessageType.Error, got.ToMessageWithType(), got);
        throw got;
      }

      return Task.FromResult(true);
    }

    public override async Task<(byte[] data, bool eof)> DownloadFileChunkAsync(string volumePath, long offset, int size)
    {
      (size < Constraints.MAX_FILE_CHUNK_SIZE).IsTrue($"size < {Constraints.MAX_FILE_CHUNK_SIZE}");
      (offset >= 0).IsTrue("offset >= 0");

      var path = getPhysicalPath(volumePath);

      try
      {
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
          if (offset >= fs.Length) return (null, true);

          fs.Position = offset;
          var buff = new byte[size];
          var eof = false;

          for(var i = 0; i < size;)
          {
            var got = await fs.ReadAsync(buff, i, buff.Length - i).ConfigureAwait(false);
            if (got == 0)
            {
              eof = true;
              break;
            }
            i += got;
          }
          return (buff, eof);
        }
      }
      catch (Exception error)
      {
        var got = new FileGatewayException($"DownloadFileChunkAsync(`{volumePath}`, {offset}, {size}): {error.Message}", error);
        WriteLogFromHere(Azos.Log.MessageType.Error, got.ToMessageWithType(), got);
        throw got;
      }
    }

    public override async Task<ItemInfo> UploadFileChunkAsync(string volumePath, long offset, byte[] content)
    {
      content.NonNull(nameof(content));
      (content.Length < Constraints.MAX_FILE_CHUNK_SIZE).IsTrue($"size < {Constraints.MAX_FILE_CHUNK_SIZE}");
      (offset >= 0).IsTrue("offset >= 0");

      var path = getPhysicalPath(volumePath);

      try
      {
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None))
        {
          fs.Position = offset;
          await fs.WriteAsync(content, 0, content.Length).ConfigureAwait(false);
        }
      }
      catch (Exception error)
      {
        var got = new FileGatewayException($"UploadFileChunkAsync(`{volumePath}`, {offset}, byte[{content.Length}]): {error.Message}", error);
        WriteLogFromHere(Azos.Log.MessageType.Error, got.ToMessageWithType(), got);
        throw got;
      }

      return getItemInfo(path);
    }

    public override Task<bool> RenameItemAsync(string volumePath, string newVolumePath)
    {
      var oldPath = getPhysicalPath(volumePath);
      var newPath = getPhysicalPath(newVolumePath);

      FileAttributes fatr;
      try
      {
        fatr = File.GetAttributes(oldPath);
      }
      catch(FileNotFoundException)
      {
        return Task.FromResult(false);
      }

      try
      {
        if (fatr.HasFlag(FileAttributes.Directory))
        {
          var di = new DirectoryInfo(oldPath);
          di.MoveTo(newPath);
        }
        else
        {
          var fi = new FileInfo(oldPath);
          fi.MoveTo(newPath);
        }
      }
      catch (Exception error)
      {
        var got = new FileGatewayException($"RenameItemAsync(`{volumePath}`, `{newVolumePath}`): {error.Message}", error);
        WriteLogFromHere(Azos.Log.MessageType.Error, got.ToMessageWithType(), got);
        throw got;
      }

      return Task.FromResult(true);
    }
  }
}
