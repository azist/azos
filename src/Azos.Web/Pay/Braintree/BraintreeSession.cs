/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Web.Pay.Braintree
{
  public class BraintreeSession : PaySession
  {
    public BraintreeSession(BraintreeSystem system, BraintreeConnectionParameters cParams, IPaySessionContext context = null)
      : base(system, cParams, context)
    {
    }

    public object ClientToken { get { return PaySystem.GenerateClientToken(this); } }

    protected new BraintreeSystem PaySystem { get { return base.PaySystem as BraintreeSystem; } }

    public string MerchantID
    {
      get
      {
        if (!IsValid) return string.Empty;
        var credentials = User.Credentials as BraintreeCredentials;
        if (credentials == null) return string.Empty;
        return credentials.MerchantID;
      }
    }
  }
}