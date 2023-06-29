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
  public sealed class DefaultFileGatewayLogic : ModuleBase, IFileGatewayLogic
  {
    public DefaultFileGatewayLogic(IApplication application) : base(application) { }
    public DefaultFileGatewayLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      //DisposeAndNull(ref X);
      base.Destructor();
    }

    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.IO_TOPIC;

    #region Protected
    protected override void DoConfigure(IConfigSectionNode node)
    {
      //base.DoConfigure(node);
      //DisposeAndNull(ref m_Server);
      //if (node == null) return;

      //var nServer = node[CONFIG_SERVICE_SECTION];
      //m_Server = FactoryUtils.MakeDirectedComponent<HttpService>(this,
      //                                                           nServer,
      //                                                           typeof(HttpService),
      //                                                           new object[] { nServer });
    }

    //check preconditions/config
    protected override bool DoApplicationAfterInit()
    {
      //m_Server.NonNull("Not configured Server of config section `{0}`".Args(CONFIG_SERVICE_SECTION));
      //GatewayServiceAddress.NonBlank(nameof(GatewayServiceAddress));
      return base.DoApplicationAfterInit();
    }

    #endregion
    #region IFileGatewayLogic

    public Task<IEnumerable<Atom>> GetSystemsAsync()
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<Atom>> GetVolumesAsync(Atom system)
    {
      throw new NotImplementedException();
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
    #endregion
  }
}
