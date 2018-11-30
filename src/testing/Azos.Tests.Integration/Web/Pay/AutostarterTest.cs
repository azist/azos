/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Apps;
using Azos.Scripting;
using Azos.Web.Pay;

namespace Azos.Tests.Integration.Web.Pay
{
  [Runnable]
  public class AutostarterTest: ExternalCfg
  {
    [Run]
    public void StripeAutostarted()
    {
      psAutostarted("Stripe");
    }

    [Run]
    public void MockAutostarted()
    {
      psAutostarted("Mock");
    }

    private void psAutostarted(string name)
    {
      using (new AzosApplication(new string[] { }, LACONF.AsLaconicConfig()))
      {
        var ps = PaySystem.Instances[name];

        Aver.IsNotNull(ps, name + " wasn't created");
        Aver.AreEqual(name, ps.Name);
      }
    }

  }
}
