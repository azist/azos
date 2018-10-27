/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.ApplicationModel;
using Azos.Web.Social;

namespace Azos.Tests.Integration.Web.Social
{
  [Runnable]
  public class AutostarterTest: ExternalCfg
  {
    [Run]
    public void SocialAutostarted()
    {
      psAutostarted(NFX_SOCIAL_PROVIDER_GP);
      psAutostarted(NFX_SOCIAL_PROVIDER_FB);
      psAutostarted(NFX_SOCIAL_PROVIDER_TWT);
      psAutostarted(NFX_SOCIAL_PROVIDER_LIN);
      psAutostarted(NFX_SOCIAL_PROVIDER_VK);
    }

    private void psAutostarted(string name)
    {
      using (new ServiceBaseApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var sn = SocialNetwork.Instances[name];

        Aver.IsNotNull(sn, name + " wasn't created");
        Aver.AreEqual(name, sn.Name);
      }
    }
  }
}
