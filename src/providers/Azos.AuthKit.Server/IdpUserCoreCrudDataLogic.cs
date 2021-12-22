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
    public IdpUserCoreCrudDataLogic(IApplication application) : base(application) { }
    public IdpUserCoreCrudDataLogic(IModule parent) : base(parent) { }

    [Inject]
    private ICrudDataStore m_Data;

    public bool IsServerImplementation => true;

    public override bool IsHardcodedModule => false;

    public override string ComponentLogTopic => throw new NotImplementedException();

    public ICryptoMessageAlgorithm MessageProtectionAlgorithm => throw new NotImplementedException();

    #region MinIdp logic portion

    //+==============================================================================================+
    //+ Template existing implementation can be looked-up from the MongoDb-related work here:        +
    //+   Type: Azos.Security.MinIdp.MinIdpMongoDbStore                                              +
    //+   File: Azos.MogoDb.dll::/Security/MinIdp/MinIdpMongoDbStore.cs                              +
    //+==============================================================================================+

    public async Task<MinIdpUserData> GetByIdAsync(Atom realm, string id, AuthenticationRequestContext ctx)
    {
      if (id.IsNullOrWhiteSpace()) return null;//bad user
      var (provider, loginType, parsedId) = parseId(id);

      parsedId = parsedId.ToLowerInvariant(); // TODO: review if this is needed?

      //Lookup by:
      //(`REALM`, `ID`, `TID`, `PROVIDER`);.
      // by executing CRUD query against ICrudDataStore (which needs to be configured as a part of this class)
      return await getByIdAsync_Implementation(realm, provider, loginType, parsedId).ConfigureAwait(false);
    }

    public async Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri, AuthenticationRequestContext ctx)
    {
      if (uri.IsNullOrWhiteSpace()) return null;//bad user

      uri = uri.ToLowerInvariant(); // TODO: review if this is needed?

      return await getByIdAsync_Implementation(realm, "default", Constraints.LTP_ID, uri);
    }

    public Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken, AuthenticationRequestContext ctx)
    {
      if (sysToken.IsNullOrWhiteSpace()) return null;//bad user

      // TODO: Add system token logic

      throw new NotImplementedException();
    }

    private async Task<MinIdpUserData> getByIdAsync_Implementation(Atom realm, string provider, Atom loginType, string parsedId)
    {
      var qry = new Query<MinIdpUserData>("UserCore.GetByIdAsync")
          {
            new Query.Param("realm", realm),
            new Query.Param("id", parsedId),
            new Query.Param("tid", loginType),
            new Query.Param("provider", provider)
          };
      return await m_Data.LoadDocAsync(qry).ConfigureAwait(false);
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
    private (string provider, Atom loginType, string parsedId) parseId(string id)
    {
      //in future, this function would be delegated to ID pre-parsing strategy which will be connected to this logic.
      //For now we are going to hard-code the rules below here

      //look for ":" - everything to the left is provider:    <provider>:<id>, if no ":" then use default provider (will be a property later)

      //parse the <id> for "@" - then it is an Constraints.LTP_EMAIL
      //otherwise try to oarse for digits, -, (phone) then it is a Constraints.LTP_PHONE
      //otherwise it is Constraints.LTP_ID

      return (null, Atom.ZERO, null);
    }

    #endregion
  }
}
