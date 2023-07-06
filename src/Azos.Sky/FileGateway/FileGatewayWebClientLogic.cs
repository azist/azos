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


namespace Azos.Sky.FileGateway
{
  /// <summary>
  /// Provides client for consuming ILogChronicle and  IInstrumentationChronicle remote services.
  /// Multiplexes reading from multiple shards when `CrossShard` filter param is passed
  /// </summary>
  public sealed class FileGatewayWebClientLogic : ModuleBase, IFileGatewayLogic
  {
    public const string CONFIG_SERVICE_SECTION = "service";

    public FileGatewayWebClientLogic(IApplication application) : base(application) { }
    public FileGatewayWebClientLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Server);
      base.Destructor();
    }

    private HttpService m_Server;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.IO_TOPIC;


    /// <summary>
    /// Logical service address of file gateway
    /// </summary>
    [Config]
    public string GatewayServiceAddress{ get; set; }


    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      DisposeAndNull(ref m_Server);
      if (node == null) return;

      var nServer = node[CONFIG_SERVICE_SECTION];
      m_Server = FactoryUtils.MakeDirectedComponent<HttpService>(this,
                                                                 nServer,
                                                                 typeof(HttpService),
                                                                 new object[] { nServer });
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Server.NonNull("Not configured Server of config section `{0}`".Args(CONFIG_SERVICE_SECTION));
      GatewayServiceAddress.NonBlank(nameof(GatewayServiceAddress));
      return base.DoApplicationAfterInit();
    }

    #region IFileGatewayLogic
    public async Task<IEnumerable<Atom>> GetSystemsAsync()
    {
      var response = await m_Server.Call(GatewayServiceAddress,
                                          nameof(IFileGatewayLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.GetJsonMapAsync("systems").ConfigureAwait(false));

      var result = response.UnwrapPayloadArray()
                           .SelectEitherOf((string str) => Atom.Encode(str), (Atom   atm) => atm);

      return result;
    }

    public async Task<IEnumerable<Atom>> GetVolumesAsync(Atom system)
    {
      system.HasRequiredValue(nameof(system));
      var uri = new UriQueryBuilder("volumes")
               .Add("system", system)
               .ToString();

      var response = await m_Server.Call(GatewayServiceAddress,
                                          nameof(IFileGatewayLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.GetJsonMapAsync(uri).ConfigureAwait(false));

      var result = response.UnwrapPayloadArray()
                           .SelectEitherOf((string str) => Atom.Encode(str), (Atom atm) => atm);

      return result;
    }

    public async Task<IEnumerable<ItemInfo>> GetItemListAsync(EntityId path, bool recurse = false)
    {
      path = Constraints.SanitizePath(path, false);

      var response = await m_Server.Call(GatewayServiceAddress,
                                          nameof(IFileGatewayLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.PostAndGetJsonMapAsync("item-list", new{ path = path, recurse = recurse}).ConfigureAwait(false));

      var result = response.UnwrapPayloadArray()
                           .OfType<JsonDataMap>()
                           .Select(imap => JsonReader.ToDoc<ItemInfo>(imap))
                           .ToArray();

      return result;
    }

    public async Task<ItemInfo> GetItemInfoAsync(EntityId path)
    {
      path = Constraints.SanitizePath(path, false);
      var uri = new UriQueryBuilder("item-info")
               .Add("path", path)
               .ToString();

      var response = await m_Server.Call(GatewayServiceAddress,
                                          nameof(IFileGatewayLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.GetJsonMapAsync(uri).ConfigureAwait(false));

      var result = JsonReader.ToDoc<ItemInfo>(response.UnwrapPayloadMap());

      return result;
    }

    public async Task<ItemInfo> CreateDirectoryAsync(EntityId path)
    {
      path = Constraints.SanitizePath(path, false);

      var response = await m_Server.Call(GatewayServiceAddress,
                                          nameof(IFileGatewayLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.PostAndGetJsonMapAsync("directory", new { path = path }).ConfigureAwait(false));

      var result = JsonReader.ToDoc<ItemInfo>(response.UnwrapPayloadMap());

      return result;
    }

    public async Task<ItemInfo> CreateFileAsync(EntityId path, CreateMode mode, long offset, byte[] content)
    {
      path = Constraints.SanitizePath(path, true);
      (offset >= 0).IsTrue("offset >= 0");
      content.NonNull(nameof(content));
      (content.Length < Constraints.MAX_FILE_CHUNK_SIZE).IsTrue($"contet.len < {Constraints.MAX_FILE_CHUNK_SIZE}");

      var response = await m_Server.Call(GatewayServiceAddress,
                                          nameof(IFileGatewayLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.PostAndGetJsonMapAsync("file",
                                            new
                                            {
                                              path = path,
                                              mode = mode,
                                              offset = offset,
                                              content = content
                                            }
                                          ,
                                          requestBixon: true).ConfigureAwait(false));

      var result = JsonReader.ToDoc<ItemInfo>(response.UnwrapPayloadMap());

      return result;
    }

    public async Task<ItemInfo> UploadFileChunkAsync(EntityId path, long offset, byte[] content)
    {
      path = Constraints.SanitizePath(path, true);
      (offset >= 0).IsTrue("offset >= 0");
      content.NonNull(nameof(content));
      (content.Length < Constraints.MAX_FILE_CHUNK_SIZE).IsTrue($"contet.len < {Constraints.MAX_FILE_CHUNK_SIZE}");

      var response = await m_Server.Call(GatewayServiceAddress,
                                          nameof(IFileGatewayLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.PutAndGetJsonMapAsync("file",
                                            new
                                            {
                                              path = path,
                                              offset = offset,
                                              content = content
                                            }
                                          ,
                                          requestBixon: true).ConfigureAwait(false));

      var result = JsonReader.ToDoc<ItemInfo>(response.UnwrapPayloadMap());

      return result;
    }

    public async Task<(byte[] data, bool eof)> DownloadFileChunkAsync(EntityId path, long offset = 0, int size = 0)
    {
      path = Constraints.SanitizePath(path, true);
      (offset >= 0).IsTrue("offset >= 0");
      (size >= 0).IsTrue("size >= 0");
      var uri = new UriQueryBuilder("file")
               .Add("path", path)
               .Add("offset", offset)
               .Add("size", size)
               .ToString();

      var response = await m_Server.Call(GatewayServiceAddress,
                                          nameof(IFileGatewayLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.GetJsonMapAsync(uri).ConfigureAwait(false));

      var resultMap = response.UnwrapPayloadMap();
      var eof = resultMap["eof"].AsBool();
      var data = resultMap["data"] as byte[];
      return (data, eof);
    }

    public async Task<bool> DeleteItemAsync(EntityId path)
    {
      path = Constraints.SanitizePath(path, false);
      var uri = new UriQueryBuilder("file")
               .Add("path", path);

      var response = await m_Server.Call(GatewayServiceAddress,
                                          nameof(IFileGatewayLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.DeleteAndGetJsonMapAsync(uri).ConfigureAwait(false));

      var resultMap = response.UnwrapPayloadMap();
      var deleted = resultMap["deleted"].AsBool();
      return deleted;
    }

    public async Task<bool> RenameItemAsync(EntityId path, string newPath)
    {
      path = Constraints.SanitizePath(path, false);
      newPath = Constraints.SanitizePath(newPath, false);

      var response = await m_Server.Call(GatewayServiceAddress,
                                          nameof(IFileGatewayLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          async (http, ct) => await http.Client.PostAndGetJsonMapAsync("item-name",
                                            new
                                            {
                                              path = path,
                                              newPath = newPath
                                            }).ConfigureAwait(false));

      var resultMap = response.UnwrapPayloadMap();
      var renamed = resultMap["renamed"].AsBool();
      return renamed;
    }
    #endregion
  }
}
