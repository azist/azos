/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Apps;
using Azos.Security.MinIdp;

namespace Azos.AuthKit
{
  public sealed class IdpUserCoreMySqlLogic : ModuleBase, IIdpUserCoreLogic
  {
    public IdpUserCoreMySqlLogic(IApplication application) : base(application) { }
    public IdpUserCoreMySqlLogic(IModule parent) : base(parent) { }

    public bool IsServerImplementation => true;

    public override bool IsHardcodedModule => false;

    public override string ComponentLogTopic => throw new NotImplementedException();
  }
}
