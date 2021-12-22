/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.AuthKit.Server
{
  public sealed class DefaultIdpHandlerLogic : ModuleBase, IIdpHandlerLogic
  {
    public DefaultIdpHandlerLogic(IApplication application) : base(application) { }
    public DefaultIdpHandlerLogic(IModule parent) : base(parent) { }

    public bool IsServerImplementation => true;
    public override bool IsHardcodedModule => false;
    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;


    public string SysTokenCryptoAlgorithmName => throw new NotImplementedException();

    public double SysTokenLifespanHours => throw new NotImplementedException();


    public string MakeSystemTokenData(GDID gUser, JsonDataMap data = null)
    {
      throw new NotImplementedException();
    }

    public (string provider, Atom loginType, string parsedId) ParseId(string id)
    {
      throw new NotImplementedException();
    }

    public (string provider, Atom loginType, string parsedUri) ParseUri(string uri)
    {
      throw new NotImplementedException();
    }
  }
}
