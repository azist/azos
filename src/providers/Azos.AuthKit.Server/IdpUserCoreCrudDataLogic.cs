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
using Azos.Security.Authkit;
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

    public static Permission[] SEC_USER_MGMT = new []
    {
      new UserManagementPermission(UserManagementAccessLevel.View)
    };

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

      setAmbientRealm(realm);

      var (pvd, eid) = m_Handler.ParseId(id);

      var actx = m_Handler.MakeNewUserAuthenticationContext(ctx);
      actx.LoginId = eid;
      actx.Provider = pvd;

      //Lookup by:
      //(`REALM`, `ID`, `TID`, `PROVIDER`);.
      // by executing CRUD query against ICrudDataStore (which needs to be configured as a part of this class)
      var qry = new Query<Doc>("MinIdp.GetById")
      {
        new Query.Param("ctx", actx)
      };

      await Data.LoadDocAsync(qry).ConfigureAwait(false);

      if (!actx.HasResult) return null;

      m_Handler.MakeSystemTokenData(actx);
      m_Handler.ApplyEffectivePolicies(actx);
      return actx.MakeResult();
    }


    public async Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri, AuthenticationRequestContext ctx)
    {
      if (uri.IsNullOrWhiteSpace()) return null;//bad user

      setAmbientRealm(realm);

      var (pvd, eid) = m_Handler.ParseUri(uri);

      var actx = m_Handler.MakeNewUserAuthenticationContext(ctx);
      actx.LoginId = eid;
      actx.Provider = pvd;

      var qry = new Query<Doc>("MinIdp.GetById")// A URI is really a special sub-type of ID Constraints.LTP_SYS_URI = 'uri'
      {
        new Query.Param("ctx", actx)
      };

      await Data.LoadDocAsync(qry).ConfigureAwait(false);

      if (!actx.HasResult) return null;

      m_Handler.MakeSystemTokenData(actx);
      m_Handler.ApplyEffectivePolicies(actx);
      return actx.MakeResult();
    }

    public async Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken, AuthenticationRequestContext ctx)
    {
      if (sysToken.IsNullOrWhiteSpace()) return null;//bad user

      setAmbientRealm(realm);

      var actx = m_Handler.MakeNewUserAuthenticationContext(ctx);

      var pvd = m_Handler.TryDecodeSystemTokenData(sysToken, actx);
      if (pvd == null) return null;//Bad token

      actx.Provider = pvd;

      var qry = new Query<Doc>("MinIdp.GetBySys")
      {
        new Query.Param("ctx", actx)
      };

      await Data.LoadDocAsync(qry).ConfigureAwait(false);

      if (!actx.HasResult) return null;

      //notice: No MakeSystemTokenData because we must re-use EXISTING token as-is
      m_Handler.ApplyEffectivePolicies(actx);
      return actx.MakeResult();
    }

    #endregion

    #region IIdpUserCoreLogic-specifics

    public async Task<IEnumerable<UserInfo>> GetUserListAsync(UserListFilter filter)
    {
      App.Authorize(SEC_USER_MGMT);

      var qry = new Query<UserInfo>("Admin.GetUserList")
      {
        new Query.Param("filter", filter)
      };

      var result = await Data.LoadEnumerableAsync(qry).ConfigureAwait(false);
      return result;
    }

    public async Task<IEnumerable<LoginInfo>> GetLoginsAsync(GDID gUser)
    {
      App.Authorize(SEC_USER_MGMT);

      var qry = new Query<LoginInfo>("Admin.GetLogins")
      {
        new Query.Param("gUser", gUser)
      };

      var result = await Data.LoadEnumerableAsync(qry).ConfigureAwait(false);
      return result;
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

    // Since realm is an implicit security context we need to set it explicitly
    private void setAmbientRealm(Atom realm) => Ambient.CurrentCallSession.DataContextName = realm.Value;

    #endregion
  }
}
