/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Collections;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Server
{
  public sealed class DefaultIdpHandlerLogic : ModuleBase, IIdpHandlerLogic
  {
    public DefaultIdpHandlerLogic(IApplication application) : base(application) { }
    public DefaultIdpHandlerLogic(IModule parent) : base(parent) { }

    Registry<LoginProvider> m_Providers;

    public bool IsServerImplementation => true;
    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    public IRegistry<LoginProvider> Providers => m_Providers;

    public string SysTokenCryptoAlgorithmName => throw new NotImplementedException();

    public double SysTokenLifespanHours => throw new NotImplementedException();


    #region Protected/Lifecycle
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
    }

    protected override bool DoApplicationAfterInit()
    {
      return base.DoApplicationAfterInit();
    }

    protected override bool DoApplicationBeforeCleanup()
    {
      return base.DoApplicationBeforeCleanup();
    }
    #endregion

    public string MakeSystemTokenData(GDID gUser, JsonDataMap data = null)
    {
      throw new NotImplementedException();
    }

    public EntityId ParseId(string id)
    {
      throw new NotImplementedException();
    }

    public EntityId ParseUri(string uri)
    {
      throw new NotImplementedException();
    }
  }
}
