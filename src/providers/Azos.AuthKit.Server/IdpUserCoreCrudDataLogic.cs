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
using Azos.Conf.Forest;
using Azos.Data;
using Azos.Data.Access;
using Azos.Data.Business;
using Azos.Security;
using Azos.Security.Authkit;
using Azos.Security.Config;
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

    public static Permission[] SEC_USER_VIEW = new []
    {
      new UserManagementPermission(UserManagementAccessLevel.View)
    };

    public static Permission[] SEC_USER_CHANGE = new[]
    {
      new UserManagementPermission(UserManagementAccessLevel.Change)
    };

    public static Permission[] SEC_USER_DELETE = new[]
    {
      new UserManagementPermission(UserManagementAccessLevel.Delete)
    };

    [Inject] IIdpHandlerLogic m_Handler;
    private ICrudDataStoreImplementation m_Data;

    [Inject] private IForestLogic m_Forest;

    private ICrudDataStore Data => m_Data.NonDisposed(nameof(m_Data));

    public bool IsServerImplementation => true;

    public override bool IsHardcodedModule => false;

    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    [Config("$msg-algo;$msg-algorithm")]
    public string MessageProtectionAlgorithmName { get; set; }

    public ICryptoMessageAlgorithm MessageProtectionAlgorithm => MessageProtectionAlgorithmName.IsNullOrWhiteSpace() ? null :
                                                                      App.SecurityManager
                                                                         .Cryptography
                                                                         .MessageProtectionAlgorithms[MessageProtectionAlgorithmName]
                                                                         .NonNull("Algo `{0}`".Args(MessageProtectionAlgorithmName))
                                                                         .IsTrue(a => a.Audience == CryptoMessageAlgorithmAudience.Internal &&
                                                                                      a.Flags.HasFlag(CryptoMessageAlgorithmFlags.Cipher) &&
                                                                                      a.Flags.HasFlag(CryptoMessageAlgorithmFlags.CanUnprotect),
                                                                                      "Algo `{0}` !internal !cipher".Args(MessageProtectionAlgorithmName));

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

    /// <inheritdoc/>
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
      await m_Handler.ApplyEffectivePoliciesAsync(actx).ConfigureAwait(false);
      return actx.MakeResult();
    }

    /// <inheritdoc/>
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
      await m_Handler.ApplyEffectivePoliciesAsync(actx).ConfigureAwait(false);
      return actx.MakeResult();
    }

    /// <inheritdoc/>
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
      await m_Handler.ApplyEffectivePoliciesAsync(actx).ConfigureAwait(false);
      return actx.MakeResult();
    }

    #endregion

    #region IIdpUserCoreLogic-specifics

    public async Task<IEnumerable<UserInfo>> GetUserListAsync(UserListFilter filter)
    {
      App.Authorize(SEC_USER_VIEW);

      var qry = new Query<UserInfo>("Admin.GetUserList")
      {
        new Query.Param("filter", filter)
      };

      var result = await Data.LoadEnumerableAsync(qry).ConfigureAwait(false);
      return result;
    }

    public async Task<IEnumerable<LoginInfo>> GetLoginsAsync(GDID gUser)
    {
      App.Authorize(SEC_USER_VIEW);

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

    public async Task<ValidState> ValidateUserAsync(UserEntity user, ValidState state)
    {
      // we will need to check user exists props, right, org unit etc.

      if (user.OrgUnit.HasValue)
      {
        using (var scope = new SecurityFlowScope(TreePermission.SYSTEM_USE_FLAG))
        {
          var idNode = m_Handler.GetIdpConfigTreeNodePath(user.Realm, user.OrgUnit);
          var tNode = await m_Forest.GetNodeInfoAsync(idNode).ConfigureAwait(false);
          if (tNode == null) state = new ValidState(state, new FieldValidationException(nameof(UserEntity), "OrgUnit was not found"));
        }
      }

      // return state
      return state;
    }

    public async Task<ChangeResult> SaveUserAsync(UserEntity user)
    {
      App.Authorize(SEC_USER_CHANGE);

      var qry = new Query<EntityChangeInfo>("Admin.SaveUser")
      {
        new Query.Param("u", user)
      };

      var change = await Data.IdpExecuteAsync(qry);
      return new ChangeResult(ChangeResult.ChangeType.Processed, 1, "Saved", change);
    }

    public Task<ValidState> ValidateLoginAsync(LoginEntity login, ValidState state)
    {
      // we will need to check user exists exists props, right, org unit etc.

      // return state
      return Task.FromResult(state);
    }

    public async Task<ChangeResult> SaveLoginAsync(LoginEntity login)
    {
      App.Authorize(SEC_USER_CHANGE);

      var qry = new Query<EntityChangeInfo>("Admin.SaveLogin")
      {
        new Query.Param("l", login)
      };

      var change = await Data.IdpExecuteAsync(qry);
      return new ChangeResult(ChangeResult.ChangeType.Processed, 1, "Saved", change);
    }

    public async Task<ChangeResult> SetLockStatusAsync(LockStatus status)
    {
      App.Authorize(SEC_USER_CHANGE);

      var qry = new Query<EntityChangeInfo>("Admin.SetLock")
      {
        new Query.Param("l", status)
      };

      var change = await Data.IdpExecuteAsync(qry);
      return new ChangeResult(ChangeResult.ChangeType.Processed, 1, "Saved", change);
    }

    #endregion

    #region pvt

    // Since realm is an implicit security context we need to set it explicitly
    private void setAmbientRealm(Atom realm) => Ambient.CurrentCallSession.DataContextName = realm.Value;

    #endregion
  }
}
