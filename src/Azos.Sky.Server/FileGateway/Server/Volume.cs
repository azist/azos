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

namespace Azos.Sky.FileGateway.Server
{
  /// <summary>
  /// Represents server gateway system volume implementation abstraction
  /// </summary>
  public abstract class Volume : ApplicationComponent<GatewaySystem>, IAtomNamed
  {
    public const string CONFIG_VOLUME_SECTION = "volume";

    internal Volume(GatewaySystem director, IConfigSectionNode conf) : base(director)
    {
      ConfigAttribute.Apply(this, conf.NonEmpty(nameof(conf)));
      m_Name.HasRequiredValue(nameof(Name));
    }

    [Config] private Atom m_Name;

    public Atom Name => m_Name;

    public override string ComponentLogTopic => ComponentDirector.ComponentLogTopic;


    public abstract Task<IEnumerable<ItemInfo>> GetItemListAsync(int recurseLevels);
    public abstract Task<ItemInfo> GetItemInfoAsync(string volumePath);
    public abstract Task<ItemInfo> CreateDirectoryAsync(string volumePath);
    public abstract Task<ItemInfo> CreateFileAsync(string volumePath, CreateMode mode, long offset, byte[] content);
    public abstract Task<bool> DeleteItemAsync(string volumePath);
    public abstract Task<(byte[] data, bool eof)> DownloadFileChunkAsync(EntityId path, long offset, int size);
    public abstract Task<bool> RenameItemAsync(EntityId path, string newPath);
    public abstract Task<ItemInfo> UploadFileChunkAsync(EntityId path, long offset, byte[] content);


  }
}
