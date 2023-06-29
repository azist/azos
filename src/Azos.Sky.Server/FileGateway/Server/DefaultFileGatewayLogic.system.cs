/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azos.Apps;
using Azos.Client;
using Azos.Collections;
using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;
using Azos.Web;

namespace Azos.Sky.FileGateway.Server
{
  partial class DefaultFileGatewayLogic
  {
    internal sealed class GatewaySystem : ApplicationComponent<DefaultFileGatewayLogic>, IAtomNamed
    {
      internal GatewaySystem(DefaultFileGatewayLogic director) : base(director)
      {
      }

      public Atom Name => throw new NotImplementedException();

      public override string ComponentLogTopic => ComponentDirector.ComponentLogTopic;
    }

    internal sealed class Volume : ApplicationComponent<GatewaySystem>, IAtomNamed
    {
      internal Volume(GatewaySystem director) : base(director)
      {
      }

      public Atom Name => throw new NotImplementedException();

      public override string ComponentLogTopic => ComponentDirector.ComponentLogTopic;
    }

  }
}
