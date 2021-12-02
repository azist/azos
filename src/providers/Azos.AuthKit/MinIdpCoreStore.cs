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
using Azos.Apps.Injection;
using Azos.Security;
using Azos.Security.MinIdp;

namespace Azos.AuthKit
{
  /// <summary>
  /// Provides <see cref="IMinIdpStoreImplementation"/> based on delegation to <see cref="IIdpUserCoreLogic"/>
  /// </summary>
  public sealed class MinIdpCoreStore : DaemonWithInstrumentation<IApplicationComponent>, IMinIdpStoreImplementation
  {
    public MinIdpCoreStore(IApplicationComponent dir) : base(dir)
    {
    }

    [Inject]
    private IIdpUserCoreLogic m_Logic;

    public ICryptoMessageAlgorithm MessageProtectionAlgorithm => throw new NotImplementedException();

    public override bool InstrumentationEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override string ComponentLogTopic => throw new NotImplementedException();

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
