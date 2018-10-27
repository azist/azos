/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


namespace Azos.Web.Pay.Braintree
{
  public class BraintreeWebTerminal : PayWebTerminal
  {
    public BraintreeWebTerminal(BraintreeSystem paySystem)
      : base(paySystem) {}

    public override object GetPayInit()
    {
      using (var session = PaySystem.StartSession())
      {
        var btSession = session as BraintreeSession;
        if (btSession == null) return string.Empty;
        return new { publicKey = btSession.ClientToken };
      }
    }
  }
}
