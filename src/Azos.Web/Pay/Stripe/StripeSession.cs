/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using Azos.Security;

namespace Azos.Web.Pay.Stripe
{
  public class StripeSession: PaySession
  {
    #region .ctor
      public StripeSession(PaySystem paySystem, StripeConnectionParameters cParams, IPaySessionContext context = null)
        : base(paySystem, cParams, context) { }
    #endregion

    #region Properties
      public string Email
      {
        get
        {
          if (!IsValid) return string.Empty;
          var cred = User.Credentials as StripeCredentials;
          if (cred == null) return string.Empty;
          return cred.Email;
        }
      }

      public string SecretKey
      {
        get
        {
          if (!IsValid) return string.Empty;
          var cred = User.Credentials as StripeCredentials;
          if (cred == null) return string.Empty;
          return cred.SecretKey;
        }
      }
    #endregion
  }
}
