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


namespace Azos.AuthKit
{
  /// <summary>
  /// Provides client for consuming IIdpUserAdminLogic remote services
  /// </summary>
  public sealed class IdpUserAdminWebClientLogic : ModuleBase, IIdpUserAdminLogic
  {
    public const string CONFIG_SERVICE_SECTION = "service";

    public IdpUserAdminWebClientLogic(IApplication application) : base(application) { }
    public IdpUserAdminWebClientLogic(IModule parent) : base(parent) { }

    protected override void Destructor()
    {
      DisposeAndNull(ref m_Server);
      base.Destructor();
    }

    private HttpService m_Server;


    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    /// <summary>
    /// Logical service address of IDP server
    /// </summary>
    [Config]
    public string IdpServiceAddress { get; set; }

    public bool IsServerImplementation => throw new NotImplementedException();

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
      IdpServiceAddress.NonBlank(nameof(IdpServiceAddress));

      return base.DoApplicationAfterInit();
    }

    public async Task<IEnumerable<UserInfo>> GetUserListAsync(UserListFilter filter)
    {
      var response = await m_Server.Call(IdpServiceAddress,
                                          nameof(IIdpUserAdminLogic),
                                          new ShardKey(0u),
                                          (http, ct) => http.Client.PostAndGetJsonMapAsync("filter", new { filter = filter })).ConfigureAwait(false);

      var result = response.UnwrapPayloadArray()
                           .OfType<JsonDataMap>()
                           .Select(imap => JsonReader.ToDoc<UserInfo>(imap));

      return result;
    }

    public async Task<IEnumerable<LoginInfo>> GetLoginsAsync(GDID gUser)
    {
      var response = await m_Server.Call(IdpServiceAddress,
                                          nameof(IIdpUserAdminLogic),
                                          new ShardKey(0u),
                                          (http, ct) => http.Client.PostAndGetJsonMapAsync("userlogins", new { gUser = gUser })).ConfigureAwait(false);

      var result = response.UnwrapPayloadArray()
                           .OfType<JsonDataMap>()
                           .Select(imap => JsonReader.ToDoc<LoginInfo>(imap));

      return result;
    }

    public async Task<ChangeResult> SaveUserAsync(UserEntity user)
    {
      var method = user.NonNull(nameof(user)).FormMode == FormMode.Insert ? System.Net.Http.HttpMethod.Post : System.Net.Http.HttpMethod.Put;

      var response = await m_Server.Call(IdpServiceAddress,
                                          nameof(IIdpUserAdminLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          (http, ct) => http.Client.CallAndGetJsonMapAsync("user", method, new { user = user })).ConfigureAwait(false);

      var result = response.UnwrapChangeResult();
      return result;
    }

    public async Task<ChangeResult> SaveLoginAsync(LoginEntity login)
    {
      var method = login.NonNull(nameof(login)).FormMode == FormMode.Insert ? System.Net.Http.HttpMethod.Post : System.Net.Http.HttpMethod.Put;

      var response = await m_Server.Call(IdpServiceAddress,
                                          nameof(IIdpUserAdminLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          (http, ct) => http.Client.CallAndGetJsonMapAsync("login", method, new { login = login })).ConfigureAwait(false);

      var result = response.UnwrapChangeResult();
      return result;
    }

    public async Task<ChangeResult> SetLockStatusAsync(LockStatus status)
    {
      var response = await m_Server.Call(IdpServiceAddress,
                                          nameof(IIdpUserAdminLogic),
                                          new ShardKey(DateTime.UtcNow),
                                          (http, ct) => http.Client.PutAndGetJsonMapAsync("lock", new { lockStatus = status })).ConfigureAwait(false);

      var result = response.UnwrapChangeResult();
      return result;
    }
  }
}
