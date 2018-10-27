/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Security;

namespace Azos.Web.Pay.PayPal
{
  /// <summary>
  /// Represents PayPal pay session
  /// </summary>
  public class PayPalSession : PaySession
  {
    public PayPalSession(PayPalSystem paySystem, PayPalConnectionParameters cParams, IPaySessionContext context = null)
        : base(paySystem, cParams, context)
    {
    }

    public PayPalOAuthToken AuthorizationToken
    {
      get
      {
        if (!IsValid) return null;

        var token = User.AuthToken.Data as PayPalOAuthToken;

        if (token == null || token.IsCloseToExpire())
        {
          token = ((PayPalSystem)PaySystem).generateOAuthToken((PayPalCredentials)User.Credentials);
          User = new User(User.Credentials, new AuthenticationToken(PayPalSystem.PAYPAL_REALM, token), User.Name, User.Rights);
        }

        return token;
      }
    }

    public void ResetAuthorizationToken()
    {
      User = new User(User.Credentials, new AuthenticationToken(PayPalSystem.PAYPAL_REALM, null), User.Name, User.Rights);
    }
  }
}
