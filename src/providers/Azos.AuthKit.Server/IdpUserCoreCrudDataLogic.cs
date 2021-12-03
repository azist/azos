/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azos.Apps;
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


    private ICrudDataStore m_Data;

    public bool IsServerImplementation => true;

    public override bool IsHardcodedModule => false;

    public override string ComponentLogTopic => throw new NotImplementedException();

    public ICryptoMessageAlgorithm MessageProtectionAlgorithm => throw new NotImplementedException();

    public Task<MinIdpUserData> GetByIdAsync(Atom realm, string id, AuthenticationRequestContext ctx)
    {
      throw new NotImplementedException();
    }

    public Task<MinIdpUserData> GetByUriAsync(Atom realm, string uri, AuthenticationRequestContext ctx)
    {
      throw new NotImplementedException();
    }

    public Task<MinIdpUserData> GetBySysAsync(Atom realm, string sysToken, AuthenticationRequestContext ctx)
    {
      throw new NotImplementedException();
    }
  }
}
