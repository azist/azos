/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Apps;
using Azos.Conf;
using Azos.Instrumentation;

namespace Azos.Security.Services
{
  public class DefaultOAuthManager : DaemonWithInstrumentation<ISecurityManager>, IOAuthManagerImplementation
  {
    public override string ComponentLogTopic => CoreConsts.SECURITY_TOPIC;

    [Config, ExternalParameter(CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION, CoreConsts.EXT_PARAM_GROUP_SECURITY)]
    public override bool InstrumentationEnabled { get; set; }



    public ISecurityManager ClientSecurity => throw new NotImplementedException();
    public ITokenRing TokenRing => throw new NotImplementedException();


  }
}
