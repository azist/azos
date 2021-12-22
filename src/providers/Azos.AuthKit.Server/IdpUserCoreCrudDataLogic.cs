/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Apps.Injection;
using Azos.AuthKit.Events;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Access;
using Azos.Security;
using Azos.Security.MinIdp;

namespace Azos.AuthKit.Server
{
  /// <summary>
  /// Embodies IIdpUserCoreLogic based on CRUD datastore solution
  /// </summary>
  public sealed class IdpUserCoreCrudDataLogic : ModuleBase, IIdpUserCoreLogic
  {
    public const string CONFIG_DATA_STORE_SECTION = "data-store";

    public IdpUserCoreCrudDataLogic(IApplication application) : base(application) { }
    public IdpUserCoreCrudDataLogic(IModule parent) : base(parent) { }

    [Inject] IIdpHandlerLogic m_Handler;
    private ICrudDataStoreImplementation m_Data;

    private ICrudDataStore Data => m_Data.NonDisposed(nameof(m_Data));

    public bool IsServerImplementation => true;

    public override bool IsHardcodedModule => false;

    public override string ComponentLogTopic => throw new NotImplementedException();

    public ICryptoMessageAlgorithm MessageProtectionAlgorithm => throw new NotImplementedException();

    #region Module Lifecycle

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      if (node==null || !node.Exists) return;

      var ndata = node[CONFIG_DATA_STORE_SECTION];
      if (!ndata.Exists) return;

      m_Data = FactoryUtils.MakeAndConfigureDirectedComponent<ICrudDataStoreImplementation>(this, ndata);
    }

    protected override bool DoApplicationAfterInit()
    {
      m_Data.NonNull("Configured {0}".Args(CONFIG_DATA_STORE_SECTION));

      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      DisposeAndNull(ref m_Data);
      return base.DoApplicationBeforeCleanup();
    }

    #endregion

    #region MinIdp logic portion

    //+==============================================================================================+
    //+ Template existing implementation can be looked-up from the MongoDb-related work here:        +
    //+   Type: Azos.Security.MinIdp.MinIdpMongoDbStore                                              +
    //+   File: Azos.MogoDb.dll::/Security/MinIdp/MinIdpMongoDbStore.cs                              +
    //+==============================================================================================+

    public async Task<MinIdpUserData> GetByIdAsync(Atom realm, string id, AuthenticationRequestContext ctx)
    {
      if (id.IsNullOrWhiteSpace()) return null;//bad user
      var (provider, loginType, parsedId) = m_Handler.ParseId(id);

      //Lookup by:
      //(`REALM`, `ID`, `TID`, `PROVIDER`);.
      // by executing CRUD query against ICrudDataStore (which needs to be configured as a part of this class)
      return await getByIdAsync_Implementation(realm, provider, loginType, parsedId).ConfigureAwait(false);
    }

    public async Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri, AuthenticationRequestContext ctx)
    {
      if (uri.IsNullOrWhiteSpace()) return null;//bad user

      var (provider, loginType, parsedId) = m_Handler.ParseUri(uri);

      return await getByIdAsync_Implementation(realm, provider, loginType, uri);
    }

    public Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken, AuthenticationRequestContext ctx)
    {
      if (sysToken.IsNullOrWhiteSpace()) return null;//bad user

      // TODO: Add system token logic

      throw new NotImplementedException();
    }

    private async Task<MinIdpUserData> getByIdAsync_Implementation(Atom realm, string provider, Atom loginType, string parsedId)
    {
      var qry = new Query<MinIdpUserData>("MinIdp.GetByIdAsync")
          {
            new Query.Param("realm", realm),
            new Query.Param("id", parsedId),
            new Query.Param("tid", loginType),
            new Query.Param("provider", provider)
          };
      return await Data.LoadDocAsync(qry).ConfigureAwait(false);
    }
    #endregion

    #region IIdpUserCoreLogic-specifics

    public Task<IEnumerable<UserInfo>> GetUserListAsync(UserListFilter filter)
    {
      throw new NotImplementedException();
    }

    public Task<IEnumerable<LoginInfo>> GetLoginsAsync(GDID gUser)
    {
      throw new NotImplementedException();
    }

    public Task<ChangeResult> ApplyLoginEventAsync(LoginEvent what)
    {
      throw new NotImplementedException();
    }

    public Task<ValidState> ValidateUserAsync(UserEntity user, ValidState state)
    {
      throw new NotImplementedException();
    }

    public Task<ChangeResult> SaveUserAsync(UserEntity user)
    {
      throw new NotImplementedException();
    }

    public Task<ValidState> ValidateLoginAsync(LoginEntity login, ValidState state)
    {
      throw new NotImplementedException();
    }

    public Task<ChangeResult> SaveLoginAsync(LoginEntity login)
    {
      throw new NotImplementedException();
    }

    public Task<ChangeResult> SetLockStatusAsync(LockStatus status)
    {
      throw new NotImplementedException();
    }

    #endregion

    #region pvt


    #endregion
  }
}
